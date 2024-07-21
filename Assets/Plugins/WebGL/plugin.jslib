mergeInto(LibraryManager.library, {
    IsPointerLocked: function () {
        return document.pointerLockElement != null
    },

    OpenFilePicker: function () {
        var fileSelect = document.querySelector('input')
        if (fileSelect == null) {
            fileSelect = document.createElement('input')
            fileSelect.type = 'file'
            fileSelect.style.display = 'none'
            fileSelect.style.visibility = 'hidden'

            fileSelect.addEventListener('change', function () {
                if (fileSelect.files.length == 1) {
                    var reader = new FileReader()
                    reader.onload = function () {
                        if (reader.result instanceof ArrayBuffer) {
                            FS.writeFile('/tmp/mapsave', new Uint8Array(reader.result))
                            SendMessage('Main Camera', 'LoadWebGLFile')
                        }
                    }
                    reader.readAsArrayBuffer(fileSelect.files[0])
                }
            })

            document.body.appendChild(fileSelect)
        }
        fileSelect.value = null;
        fileSelect.click()
    },
})
