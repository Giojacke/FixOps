window.chartInstances = {};

window.renderChart = (canvasId, config) => {
    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
    }
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    window.chartInstances[canvasId] = new Chart(ctx, config);
};

window.destroyChart = (canvasId) => {
    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
        delete window.chartInstances[canvasId];
    }
};
