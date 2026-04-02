mergeInto(LibraryManager.library, {
  CopyToClipboardWebGL: function(text) {
    var str = UTF8ToString(text);
    navigator.clipboard.writeText(str).then(function() {
      console.log("Room code copied to clipboard: " + str);
    }).catch(function(err) {
      console.error("Failed to copy to clipboard: " + err);
    });
  }
});