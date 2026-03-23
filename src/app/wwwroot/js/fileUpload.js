window.AbbaFleet = window.AbbaFleet || {};

/**
 * Uploads a file via HTTP fetch to the API endpoint.
 * Bypasses Blazor Server's BrowserFileStream (SignalR JS-to-.NET streaming pipe)
 * which suffers from RemoteJSDataStream timeout issues in containerised environments.
 *
 * @param {HTMLElement} container - The wrapper element containing the <input type="file">.
 * @param {string} entityType - "Driver" or "Truck".
 * @param {string} entityId - The GUID of the parent entity.
 * @returns {Promise<object|null>} The uploaded FileDto or { error: string }.
 */
AbbaFleet.uploadFileFromInput = async function (container, entityType, entityId) {
    const input = container.querySelector('input[type="file"]');
    if (!input || !input.files || input.files.length === 0) {
        return { error: 'No file selected.' };
    }

    const file = input.files[0];
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', entityType);
    formData.append('entityId', entityId);

    const response = await fetch('/api/files/upload', {
        method: 'POST',
        body: formData
    });

    if (!response.ok) {
        const text = await response.text();
        return { error: text || 'Upload failed.' };
    }

    return await response.json();
};
