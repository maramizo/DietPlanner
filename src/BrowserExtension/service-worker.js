const NATIVE_HOST = "com.dietplanner.recipe_importer";
const MAX_HTML_CHARACTERS = 5_000_000;
const MAX_SAVED_JOBS = 100;

let nativePort = null;
const pendingJobs = new Set();
let storageQueue = Promise.resolve();
let idleDisconnectTimer = null;

function mutateJobs(mutator) {
  const mutation = storageQueue
    .catch(() => undefined)
    .then(async () => {
      const stored = await chrome.storage.local.get("recipeJobs");
      const jobs = { ...(stored.recipeJobs || {}) };
      await mutator(jobs);

      const entries = Object.entries(jobs).sort(
        (left, right) => (right[1].startedAt || 0) - (left[1].startedAt || 0)
      );
      const retained = {};
      let retainedCount = 0;
      for (const [jobId, job] of entries) {
        if (job.status === "in_progress" || retainedCount < MAX_SAVED_JOBS) {
          retained[jobId] = job;
          if (job.status !== "in_progress") retainedCount += 1;
        }
      }

      await chrome.storage.local.set({ recipeJobs: retained });
      await updateBadge(retained);
    });
  storageQueue = mutation;
  return mutation;
}

async function updateBadge(jobs) {
  const activeCount = Object.values(jobs).filter(
    (job) => job.status === "in_progress"
  ).length;
  await chrome.action.setBadgeBackgroundColor({ color: "#b7791f" });
  await chrome.action.setBadgeText({
    text: activeCount > 0 ? String(activeCount) : ""
  });
}

function updateJob(jobId, values) {
  return mutateJobs((jobs) => {
    if (!jobs[jobId]) return;
    jobs[jobId] = {
      ...jobs[jobId],
      ...values,
      updatedAt: Date.now()
    };
  });
}

function addPendingJob(jobId, url) {
  return mutateJobs((jobs) => {
    jobs[jobId] = {
      jobId,
      url,
      status: "in_progress",
      message: "Reading the current page…",
      error: "",
      startedAt: Date.now(),
      updatedAt: Date.now()
    };
  });
}

function getNativePort() {
  if (nativePort) return nativePort;

  const port = chrome.runtime.connectNative(NATIVE_HOST);
  nativePort = port;
  port.onMessage.addListener((message) => {
    if (!message || message.type !== "status" || !message.jobId) return;

    const values = {
      status: message.status || "failed",
      message: message.message || "",
      error: message.error || "",
      recipeName: message.recipeName || "",
      alreadyExists: Boolean(message.alreadyExists)
    };
    const statusUpdate = updateJob(message.jobId, values).catch(() => undefined);
    if (message.status === "completed" || message.status === "failed") {
      pendingJobs.delete(message.jobId);
      statusUpdate.finally(() => disconnectNativePortWhenIdle(port));
    }
  });
  port.onDisconnect.addListener(() => {
    if (nativePort !== port) return;
    nativePort = null;
    const nativeError = chrome.runtime.lastError?.message;
    const error = nativeError
      ? `Could not connect to the DietPlanner app: ${nativeError}`
      : "The DietPlanner app connection closed before this recipe finished.";
    const interruptedJobs = [...pendingJobs];
    pendingJobs.clear();
    if (interruptedJobs.length === 0) return;

    mutateJobs((jobs) => {
      for (const jobId of interruptedJobs) {
        if (!jobs[jobId] || jobs[jobId].status !== "in_progress") continue;
        jobs[jobId] = {
          ...jobs[jobId],
          status: "failed",
          message: "Import failed",
          error,
          updatedAt: Date.now()
        };
      }
    }).catch(() => undefined);
  });
  return port;
}

function disconnectNativePortWhenIdle(port) {
  if (idleDisconnectTimer !== null) clearTimeout(idleDisconnectTimer);
  idleDisconnectTimer = setTimeout(() => {
    idleDisconnectTimer = null;
    if (nativePort !== port || pendingJobs.size > 0) return;
    nativePort = null;
    port.disconnect();
  }, 1000);
}

async function capturePage(tabId) {
  const results = await chrome.scripting.executeScript({
    target: { tabId },
    func: (maximumCharacters) => {
      const html = document.documentElement?.outerHTML || "";
      return html.slice(0, maximumCharacters);
    },
    args: [MAX_HTML_CHARACTERS]
  });
  return results[0]?.result || "";
}

async function enqueueCurrentPage(message) {
  const jobId = crypto.randomUUID();
  const url = String(message.url || "");
  await addPendingJob(jobId, url || "Unknown page");

  try {
    const parsedUrl = new URL(url);
    if (parsedUrl.protocol !== "http:" && parsedUrl.protocol !== "https:") {
      throw new Error("Open a public HTTP or HTTPS recipe page before using DietPlanner.");
    }

    const html = await capturePage(message.tabId);
    if (!html.trim()) {
      throw new Error("The browser did not expose any page HTML to DietPlanner.");
    }

    await updateJob(jobId, {
      message: "DietPlanner is extracting this recipe…"
    });
    if (idleDisconnectTimer !== null) {
      clearTimeout(idleDisconnectTimer);
      idleDisconnectTimer = null;
    }
    pendingJobs.add(jobId);
    const port = getNativePort();
    port.postMessage({
      type: "add_recipe",
      jobId,
      url,
      html
    });
  } catch (error) {
    pendingJobs.delete(jobId);
    await updateJob(jobId, {
      status: "failed",
      message: "Import failed",
      error: error.message || String(error)
    });
  }

  return { ok: true, jobId };
}

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message?.type === "enqueue_current_page") {
    enqueueCurrentPage(message)
      .then(sendResponse)
      .catch((error) => sendResponse({ ok: false, error: error.message }));
    return true;
  }

  if (message?.type === "clear_finished") {
    mutateJobs((jobs) => {
      for (const [jobId, job] of Object.entries(jobs)) {
        if (job.status !== "in_progress") delete jobs[jobId];
      }
    })
      .then(() => sendResponse({ ok: true }))
      .catch((error) => sendResponse({ ok: false, error: error.message }));
    return true;
  }

  sendResponse({ ok: false, error: "Unknown extension request." });
  return false;
});

chrome.runtime.onStartup.addListener(() => {
  mutateJobs((jobs) => {
    for (const job of Object.values(jobs)) {
      if (job.status !== "in_progress") continue;
      job.status = "failed";
      job.message = "Import interrupted";
      job.error = "The browser closed before DietPlanner finished this recipe.";
      job.updatedAt = Date.now();
    }
  }).catch(() => undefined);
});

chrome.runtime.onInstalled.addListener(() => {
  chrome.storage.local.get("recipeJobs").then((stored) => {
    return updateBadge(stored.recipeJobs || {});
  }).catch(() => undefined);
});
