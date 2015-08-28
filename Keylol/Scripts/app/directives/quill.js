(function() {
	"use strict";

	keylolApp.directive("quill", [
		function() {
			return {
				restrict: "A",
				require: "ngModel",
				scope: {
					content: "=ngModel"
				},
				transclude: true,
				templateUrl: "Templates/Directives/quill.html",
				link: function(scope, iElement, iAttrs, ngModel) {
					var options = {
						modules: {
							toolbar: { container: null },
							"click-expand": true,
							"float-toolbar": true
						},
						theme: "snow",
						formats: ["bold", "italic", "underline", "link", "image"]
					};
					if (iAttrs.quill)
						$.extend(options, scope.$eval(iAttrs.quill));
					var toolbar = iElement.find("[quill-toolbar]")[0];
					if (toolbar) {
						options.modules.toolbar.container = toolbar;
					} else {
						delete options.modules.toolbar;
					}
					var contentArea = iElement.find("[quill-content]")[0];
					var quill = new Quill(contentArea, options);
					quill.addFormat("blockquote", { tag: "BLOCKQUOTE", type: "line", exclude: "subtitle" });
					quill.addFormat("subtitle", { tag: "H1", prepare: "heading", type: "line", exclude: "blockquote" });
					if (!scope.content) {
						ngModel.$setViewValue(quill.getHTML());
					}
					ngModel.$render = function() {
						quill.setHTML(ngModel.$viewValue || "");
					};

					quill.on("text-change", function() {
						ngModel.$setViewValue(quill.getHTML());
					});
				}
			};
		}
	]);
})();