(function() {
	Quill.registerModule("click-expand", function(quill, options) {
		var $container = $(quill.container);
		var $editorArea = $(quill.editor.root);
		var $tip = $("<div class='ql-click-expand-tip'>双击文本框扩展/收缩编辑区域</div>");
		$container.append($tip);
		$tip.delay("3000").fadeOut();
		$container.dblclick(function() {
			if ($container.hasClass("ql-auto-expand")) {
				$container.removeClass("ql-auto-expand");
			} else {
				$editorArea.css("min-height", $container.css("height"));
				$container.addClass("ql-auto-expand");
			}
		});
	});
})();