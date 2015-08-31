(function() {
	"use strict";

	keylolApp.directive("webpBackground", [
		"utils",
		function(utils) {
			return {
				restrict: "A",
				priority: 98,
				link: function(scope, iElement, iAttrs) {
					iAttrs.$observe("webpBackground", function(value) {
						utils.supportWebp.then(function() {
							iElement.css("background-image", "url(" + value + ".webp" + ")");
						}, function() {
							iElement.css("background-image", "url(" + value + ")");
						});
					});
				}
			};
		}
	]);
})();