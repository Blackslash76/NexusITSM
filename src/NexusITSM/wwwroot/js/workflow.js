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
            const x = Math.max(0, e.clientX - canvasRect.left - offsetX);
            const y = Math.max(0, e.clientY - canvasRect.top - offsetY);
            dragNode.style.left = x + 'px';
            dragNode.style.top = y + 'px';
            e.preventDefault();
        });

        document.addEventListener('mouseup', (e) => {
            if (!dragNode) return;
            const canvasRect = canvas.getBoundingClientRect();
            const x = Math.max(0, e.clientX - canvasRect.left - offsetX);
            const y = Math.max(0, e.clientY - canvasRect.top - offsetY);
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
            const x = Math.round(e.clientX - canvasRect.left - 80);
            const y = Math.round(e.clientY - canvasRect.top - 20);
            dotnetRef.invokeMethodAsync('OnPaletteDrop', Math.max(0, x), Math.max(0, y));
        });
    }
};
