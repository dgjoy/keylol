(function() {
	Quill.registerModule("float-toolbar", function(quill, options) {
		var $container = $(quill.container);
		var $editorArea = $(quill.editor.root);
		var $toolbar = $(quill.modules.toolbar.container);
		var $formatGroups = $toolbar.find(".ql-format-group");

		var scrollParent = $toolbar[0];
		while (scrollParent.offsetParent) {
			scrollParent = scrollParent.offsetParent;
		}

		var offsetTop = function(elem) {
			if (!elem) elem = this;

			var y = elem.offsetTop;

			while (elem = elem.offsetParent) {
				y += elem.offsetTop;
			}

			return y;
		};

		var jumpPositionTop = offsetTop($toolbar[0]);
		var $editorParent = $container.parent();
		$editorParent.css("position", "relative");

		$(scrollParent).scroll(function() {
			var scrollValue = scrollParent.scrollTop;
			var jumpPositionBottom = offsetTop($editorArea[0]) + $editorArea[0].offsetHeight - $toolbar[0].offsetHeight;

			if (scrollValue < jumpPositionTop) {
				$toolbar[0].style.position = "static";
				$editorParent[0].style.paddingTop = 0;
				$formatGroups.removeClass("ql-format-group-flying");
			} else {
				$editorParent[0].style.paddingTop = $toolbar[0].offsetHeight + "px";
				$toolbar[0].style.width = $editorParent[0].offsetWidth + "px";
				$formatGroups.addClass("ql-format-group-flying");
				if (scrollValue < jumpPositionBottom) {
					$toolbar[0].style.position = "fixed";
					$toolbar[0].style.top = 0;
				} else {
					$toolbar[0].style.position = "absolute";
					$toolbar[0].style.top = "calc(100% - " + $toolbar[0].offsetHeight + "px)";
				}
			}
		});
	});
})();