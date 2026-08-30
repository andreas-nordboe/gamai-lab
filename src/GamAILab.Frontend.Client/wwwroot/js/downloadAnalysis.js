window.downloadFile = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
};

// For Lincoln assessment marker: I sourced this from here: https://learn.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-7.0#download-from-a-stream
// which is standard code typically used to download files from a stream using Blazor.