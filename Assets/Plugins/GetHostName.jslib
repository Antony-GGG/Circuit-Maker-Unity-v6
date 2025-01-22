mergeInto(LibraryManager.library, {
    GetHostname: function() {
        // Retrieve the hostname and convert it to a string Unity can read
        var hostname = window.location.hostname;
        var length = lengthBytesUTF8(hostname) + 1;
        var buffer = _malloc(length);
        stringToUTF8(hostname, buffer, length);
        return buffer;
    }
});