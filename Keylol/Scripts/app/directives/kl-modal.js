(function() {
	"use strict";

	keylolApp.directive("klModal", [
		"$timeout", function($timeout) {
			var exitFocus;
			return {
				restrict: "E",
				transclude: true,
				templateUrl: "Templates/Modal/_container.html",
				scope: {
					position: "@position"
				},
				compile: function() {
					return {
						post: function postLink(scope, iElement) {
							exitFocus = angular.element(iElement[0].querySelector(".modal-exit-button"));
							iElement.on("click", function(e) {
								if (e.target === iElement[0]) {
									if (!exitFocus.hasClass("focused")) {
										exitFocus.addClass("focused");
										$timeout(function() {
											exitFocus.removeClass("focused");
										}, 900);
									}
								}
							});
						}
					};
				}
			};
		}
	]);
})();