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
            const x = Math.max(0, Math.min(canvasRect.width - 160, e.clientX - canvasRect.left - offsetX));
            const y = Math.max(0, Math.min(canvasRect.height - 50, e.clientY - canvasRect.top - offsetY));
            dragNode.style.left = x + 'px';
            dragNode.style.top = y + 'px';

            // Update SVG connections in real-time
            updateConnections(canvas);
            e.preventDefault();
        });

        document.addEventListener('mouseup', (e) => {
            if (!dragNode) return;
            const canvasRect = canvas.getBoundingClientRect();
            const x = Math.max(0, Math.min(canvasRect.width - 160, e.clientX - canvasRect.left - offsetX));
            const y = Math.max(0, Math.min(canvasRect.height - 50, e.clientY - canvasRect.top - offsetY));
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

function updateConnections(canvas) {
    const svg = canvas.querySelector('svg');
    if (!svg) return;

    const nodes = canvas.querySelectorAll('.wf-node');
    const nodeMap = {};
    nodes.forEach(n => {
        const id = n.dataset.nodeId;
        if (id) {
            nodeMap[id] = {
                x: parseFloat(n.style.left) || 0,
                y: parseFloat(n.style.top) || 0,
                w: n.offsetWidth || 160,
                h: n.offsetHeight || 50
            };
        }
    });

    // Update all path and circle elements that represent connections
    const paths = svg.querySelectorAll('path[data-from]');
    paths.forEach(path => {
        const fromId = path.dataset.from;
        const toId = path.dataset.to;
        const from = nodeMap[fromId];
        const to = nodeMap[toId];
        if (!from || !to) return;

        const x1 = from.x + from.w / 2;
        const y1 = from.y + from.h / 2;
        const x2 = to.x + to.w / 2;
        const y2 = to.y + to.h / 2;
        const cx = (x1 + x2) / 2;

        path.setAttribute('d', `M ${x1} ${y1} C ${cx} ${y1}, ${cx} ${y2}, ${x2} ${y2}`);

        // Update arrow circle
        const circle = path.nextElementSibling;
        if (circle && circle.tagName === 'circle') {
            circle.setAttribute('cx', x2);
            circle.setAttribute('cy', y2);
        }

        // Update label
        const label = circle ? circle.nextElementSibling : null;
        if (label && label.tagName === 'text') {
            label.setAttribute('x', cx);
            label.setAttribute('y', (y1 + y2) / 2 - 6);
        }
    });
}
