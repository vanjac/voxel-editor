mergeInto(LibraryManager.library, {
    IsPointerLocked: function() {
        return document.pointerLockElement != null
    },
})
