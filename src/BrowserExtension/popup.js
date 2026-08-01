const addButton = document.querySelector("#add-current");
const clearButton = document.querySelector("#clear-finished");
const actionMessage = document.querySelector("#action-message");
const emptyState = document.querySelector("#empty-state");
const jobList = document.querySelector("#job-list");

function sendMessage(message) {
  return new Promise((resolve, reject) => {
    chrome.runtime.sendMessage(message, (response) => {
      if (chrome.runtime.lastError) {
        reject(new Error(chrome.runtime.lastError.message));
        return;
      }
      resolve(response);
    });
  });
}

function queryCurrentTab() {
  return new Promise((resolve, reject) => {
    chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
      if (chrome.runtime.lastError) {
        reject(new Error(chrome.runtime.lastError.message));
        return;
      }
      resolve(tabs[0]);
    });
  });
}

function statusIcon(status) {
  if (status === "completed") return "✓";
  if (status === "failed") return "✕";
  return "⏳";
}

function statusLabel(job) {
  if (job.status === "completed") {
    return job.message || "Recipe added to DietPlanner.";
  }
  if (job.status === "failed") return "Import failed";
  return job.message || "DietPlanner is processing this page…";
}

function renderJobs(jobsById) {
  const jobs = Object.values(jobsById || {}).sort(
    (left, right) => (right.startedAt || 0) - (left.startedAt || 0)
  );
  jobList.replaceChildren();
  emptyState.hidden = jobs.length > 0;
  clearButton.disabled = !jobs.some((job) => job.status !== "in_progress");

  for (const job of jobs) {
    const item = document.createElement("li");
    item.className = `job ${job.status || "in_progress"}`;

    const icon = document.createElement("span");
    icon.className = "status-icon";
    icon.setAttribute("aria-hidden", "true");
    icon.textContent = statusIcon(job.status);

    const details = document.createElement("div");
    if (job.status === "completed" && job.recipeName) {
      const recipeName = document.createElement("span");
      recipeName.className = "job-recipe-name";
      recipeName.textContent = job.recipeName;
      details.append(recipeName);
    }

    const url = document.createElement("span");
    url.className = "job-url";
    url.title = job.url || "Unknown page";
    url.textContent = job.url || "Unknown page";

    const message = document.createElement("p");
    message.className = "job-message";
    message.textContent = statusLabel(job);

    details.append(url, message);
    if (job.status === "failed" && job.error) {
      const error = document.createElement("p");
      error.className = "job-error";
      error.textContent = job.error;
      details.append(error);
    }
    item.append(icon, details);
    jobList.append(item);
  }
}

async function refreshJobs() {
  const result = await chrome.storage.local.get("recipeJobs");
  renderJobs(result.recipeJobs);
}

addButton.addEventListener("click", async () => {
  addButton.disabled = true;
  actionMessage.textContent = "";
  try {
    const tab = await queryCurrentTab();
    if (!tab || typeof tab.id !== "number") {
      throw new Error("DietPlanner could not identify the current tab.");
    }
    const response = await sendMessage({
      type: "enqueue_current_page",
      tabId: tab.id,
      url: tab.url || ""
    });
    if (!response || !response.ok) {
      throw new Error(response?.error || "DietPlanner could not queue this page.");
    }
    actionMessage.textContent = "Page queued. You may open another tab and add it now.";
    actionMessage.style.color = "#397358";
  } catch (error) {
    actionMessage.textContent = error.message;
    actionMessage.style.color = "#9b2c2c";
  } finally {
    addButton.disabled = false;
  }
});

clearButton.addEventListener("click", async () => {
  actionMessage.textContent = "";
  try {
    await sendMessage({ type: "clear_finished" });
  } catch (error) {
    actionMessage.textContent = error.message;
    actionMessage.style.color = "#9b2c2c";
  }
});

chrome.storage.onChanged.addListener((changes, areaName) => {
  if (areaName === "local" && changes.recipeJobs) {
    renderJobs(changes.recipeJobs.newValue);
  }
});

refreshJobs().catch((error) => {
  actionMessage.textContent = error.message;
});
