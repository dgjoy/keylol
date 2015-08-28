(function() {
	"use strict";

	keylolApp.directive("quill", [
		function() {
			return {
				restrict: "A",
				link: function(scope, iElement, iAttrs) {
					var options = {
						modules: {
							toolbar: { container: null },
							"click-expand": true,
							"float-toolbar": true
						},
						theme: "snow"
					}
					if (iAttrs.quill)
						$.extend(options, scope.$eval(iAttrs.quill));
					var toolbar = iElement.find("[quill-toolbar]")[0];
					if (toolbar) {
						options.modules.toolbar.container = toolbar;
					} else {
						delete options.modules.toolbar;
					}
					var contentArea = document.createElement("div");
					iElement.append(contentArea);
					var quill = new Quill(contentArea, options);
					quill.addFormat("blockquote", {tag: "BLOCKQUOTE", type: "line", exclude: "subtitle"});
					quill.addFormat("subtitle", {tag: "H1", prepare: "heading", type: "line", exclude: "blockquote"});
				}
			};
		}
	]);
})();