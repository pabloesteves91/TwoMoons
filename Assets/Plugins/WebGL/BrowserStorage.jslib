// Speicherstände im localStorage des Browsers. Anders als PlayerPrefs hängt der localStorage nur an der Domain
// (z. B. html-classic.itch.zone) und nicht am Pfad – so bleiben Spielstände bei einem neuen Upload auf itch.io erhalten.
mergeInto(LibraryManager.library, {
  BrowserStorage_Set: function (key, value) {
    try {
      localStorage.setItem(UTF8ToString(key), UTF8ToString(value));
      return 1;
    } catch (e) {
      return 0;
    }
  },

  BrowserStorage_Get: function (key) {
    var value = null;
    try { value = localStorage.getItem(UTF8ToString(key)); } catch (e) { }
    if (value === null) return null;
    var size = lengthBytesUTF8(value) + 1;
    var buffer = _malloc(size);
    stringToUTF8(value, buffer, size);
    return buffer;
  },

  BrowserStorage_Has: function (key) {
    try { return localStorage.getItem(UTF8ToString(key)) !== null ? 1 : 0; } catch (e) { return 0; }
  },

  BrowserStorage_Remove: function (key) {
    try { localStorage.removeItem(UTF8ToString(key)); } catch (e) { }
  },
});
