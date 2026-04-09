window.workflowInterop = {
    initDrag: function (dotnetRef) {
        const canvas = document.getElementById('wf-canvas');
        if (!canvas) return;

        let dragNode = null;
        let offsetX = 0, offsetY = 0;

        canvas.addEventListener('mousedown', (e) => {
            const node = e.target.closest('.wf-node');
            if (!node || e.target.closest('.wf-connect-dot')) return;

            dragNode = node;
            const rect = node.getBoundingClientRect();
            offsetX = e.clientX - rect.left;
            offsetY = e.clientY - rect.top;
            node.style.zIndex = '10';
            node.style.opacity = '0.9';
            e.preventDefault();
        });

        document.addEventListener('mousemove', (e) => {
            if (!dragNode) return;
            const canvasRect = canvas.getBoundingClientRect();
            const x = Math.max(0, Math.min(canvasRect.width - 170, e.clientX - canvasRect.left - offsetX));
            const y = Math.max(0, Math.min(canvasRect.height - 60, e.clientY - canvasRect.top - offsetY));
            dragNode.style.left = x + 'px';
            dragNode.style.top = y + 'px';
            updateAllConnections();
            e.preventDefault();
        });

        document.addEventListener('mouseup', (e) => {
            if (!dragNode) return;
            const canvasRect = canvas.getBoundingClientRect();
            const x = Math.max(0, Math.min(canvasRect.width - 170, e.clientX - canvasRect.left - offsetX));
            const y = Math.max(0, Math.min(canvasRect.height - 60, e.clientY - canvasRect.top - offsetY));
            const nodeId = dragNode.dataset.nodeId;
            dragNode.style.zIndex = '2';
            dragNode.style.opacity = '1';
            dragNode = null;

            if (nodeId) {
                dotnetRef.invokeMethodAsync('OnNodeMoved', nodeId, Math.round(x), Math.round(y));
            }
        });

        // Palette drag & drop
        canvas.addEventListener('dragover', (e) => { e.preventDefault(); });
        canvas.addEventListener('drop', (e) => {
            e.preventDefault();
            const canvasRect = canvas.getBoundingClientRect();
            const x = Math.round(Math.max(0, e.clientX - canvasRect.left - 80));
            const y = Math.round(Math.max(0, e.clientY - canvasRect.top - 20));
            dotnetRef.invokeMethodAsync('OnPaletteDrop', x, y);
        });
    }
};

function updateAllConnections() {
    const canvas = document.getElementById('wf-canvas');
    if (!canvas) return;

    // Build map of node positions from actual DOM
    const nodeMap = {};
    canvas.querySelectorAll('.wf-node').forEach(n => {
        const id = n.dataset.nodeId;
        if (id) {
            nodeMap[id] = {
                cx: (parseFloat(n.style.left) || 0) + (n.offsetWidth / 2),
                cy: (parseFloat(n.style.top) || 0) + (n.offsetHeight / 2)
            };
        }
    });

    // Update every connection path
    canvas.querySelectorAll('path[data-from]').forEach(path => {
        const from = nodeMap[path.dataset.from];
        const to = nodeMap[path.dataset.to];
        if (!from || !to) return;

        const mx = (from.cx + to.cx) / 2;
        path.setAttribute('d', `M ${from.cx} ${from.cy} C ${mx} ${from.cy}, ${mx} ${to.cy}, ${to.cx} ${to.cy}`);

        // Update the arrow dot (next sibling circle)
        const circle = path.nextElementSibling;
        if (circle && circle.tagName === 'circle') {
            circle.setAttribute('cx', to.cx);
            circle.setAttribute('cy', to.cy);
        }

        // Update the label (sibling after circle)
        const label = circle ? circle.nextElementSibling : null;
        if (label && label.tagName === 'text') {
            label.setAttribute('x', mx);
            label.setAttribute('y', (from.cy + to.cy) / 2 - 8);
        }
    });
}
