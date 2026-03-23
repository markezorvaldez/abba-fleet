window.fileUpload = {
    init: function (inputId, entityType, entityId, dotNetRef) {
        const input = document.getElementById(inputId);
        if (!input) return;

        input.addEventListener('change', async function () {
            if (!input.files || input.files.length === 0) return;

            const file = input.files[0];
            const formData = new FormData();
            formData.append('file', file);
            formData.append('entityType', entityType);
            formData.append('entityId', entityId);

            dotNetRef.invokeMethodAsync('OnUploadStarted');

            try {
                const response = await fetch('/api/files/upload', {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    await dotNetRef.invokeMethodAsync('OnFileUploaded');
                } else {
                    const text = await response.text();
                    await dotNetRef.invokeMethodAsync('OnUploadFailed', text || 'Upload failed');
                }
            } catch (e) {
                await dotNetRef.invokeMethodAsync('OnUploadFailed', e.message || 'Upload failed');
            }

            // Reset so the same file can be re-uploaded
            input.value = '';
        });
    },

    trigger: function (inputId) {
        const input = document.getElementById(inputId);
        if (input) input.click();
    }
};
