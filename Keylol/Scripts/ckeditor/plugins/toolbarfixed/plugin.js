/*
*   Plugin developed by Netbroad, C.B.
*
*   LICENCE: GPL, LGPL, MPL
*   NON-COMMERCIAL PLUGIN.
*
*   Website: netbroad.eu
*   Twitter: @netbroadcb
*   Facebook: Netbroad
*   LinkedIn: Netbroad
*
*/

CKEDITOR.plugins.add( 'toolbarfixed', {
	requires: "toolbar",
    init: function(editor) {
		editor.on("uiReady", function() {
			var content = document.getElementsByClassName('cke_contents').item(0);
			var toolbar = document.getElementsByClassName('cke_top').item(0);
			var editor = document.getElementsByClassName('cke').item(0);
			var inner = document.getElementsByClassName('cke_inner').item(0);
			toolbar.style.boxSizing = "border-box";

			var scrollParent = toolbar;
			while ( scrollParent.offsetParent ) {
				scrollParent = scrollParent.offsetParent;
			}
			function offset(elem) {
				if(!elem) elem = this;

				var x = elem.offsetLeft;
				var y = elem.offsetTop;

				while (elem = elem.offsetParent) {
					x += elem.offsetLeft;
					y += elem.offsetTop;
				}

				return { left: x, top: y };
			}

			scrollParent.addEventListener('scroll', function() {
				var scrollvalue = scrollParent.scrollTop;

				toolbar.style.width = content.offsetWidth + "px";
				toolbar.style.top = "0";

				if(toolbar.offsetTop <= scrollvalue){
					toolbar.style.position   = "fixed";
					content.style.paddingTop = toolbar.offsetHeight + "px";
				}

				if(editor.offsetTop > scrollvalue && (editor.offsetTop + editor.offsetHeight) >= (scrollvalue + toolbar.offsetHeight)){
					toolbar.style.position   = "relative";
					content.style.paddingTop = "0px";
				}

				if((editor.offsetTop + editor.offsetHeight) < (scrollvalue + toolbar.offsetHeight)){
					toolbar.style.position = "absolute";
					toolbar.style.top      = "calc(100% - " + toolbar.offsetHeight + "px)";
					inner.style.position   = "relative";
				}
			}, false);
        });
    }
});