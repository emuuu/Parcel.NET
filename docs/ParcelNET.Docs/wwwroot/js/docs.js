window.ParcelNetDocs = {
    highlightAll: function () {
        if (typeof Prism !== 'undefined') {
            requestAnimationFrame(function () {
                Prism.highlightAll();
            });
        }
    },

    highlightElement: function (element) {
        if (typeof Prism !== 'undefined' && element) {
            Prism.highlightElement(element);
        }
    },

    copyToClipboard: function (text) {
        if (navigator.clipboard) {
            return navigator.clipboard.writeText(text);
        }
    },

    registerSearchShortcut: function (dotNetRef) {
        document.addEventListener('keydown', function (e) {
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('Open');
            }
        });
    },

    focusElement: function (element) {
        if (element) {
            setTimeout(function () { element.focus(); }, 50);
        }
    },

    scrollNavToActive: function () {
        var active = document.querySelector('.docs-sidebar .nav-link.active');
        if (active) {
            active.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
        }
    },

    scrollToElement: function (id) {
        var element = document.getElementById(id);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    },

    downloadBase64File: function (dataUrl, filename) {
        var a = document.createElement('a');
        a.href = dataUrl;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }
};
