(function() {
	"use strict";

	keylolApp.directive("focusMe", [
		"$parse",
		function($parse) {
			return {
				link: function(scope, element, attrs) {
					scope.$watch(attrs.focusMe, function(value) {
						if (value === true) {
							element[0].focus();
							$parse(attrs.focusMe).assign(scope, false);
						}
					});
				}
			};
		}
	]);
})();