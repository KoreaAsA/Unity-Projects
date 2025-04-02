mergeInto(LibraryManager.library, {
    _checkMobileBrowser: function () {
        if (navigator.userAgent.match(/Mobi/)) {
            return 1;
        } else {
            return 0;
        }
    },
});