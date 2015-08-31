(function() {
	"use strict";

	keylolApp.directive("webpSrc", [
		"utils",
		function(utils) {
			return {
				restrict: "A",
				priority: 98,
				link: function(scope, iElement, iAttrs) {
					iAttrs.$observe("webpSrc", function(value) {
						utils.supportWebp.then(function() {
							iAttrs.$set("src", value + ".webp");
						}, function() {
							iAttrs.$set("src", value);
						});
					});
				}
			};
		}
	]);
})();