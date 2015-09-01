(function() {
	"use strict";

	keylolApp.directive("inputCode", [
		function() {
			return {
				restrict: "E",
				templateUrl: "Templates/Directives/input-code.html",
				scope: {
					length: "="
				},
				require: "ngModel",
				link: function(scope, iElement, iAttrs, ngModel) {
					scope.getNumber = function(num) {
						return new Array(num);
					};
					scope.text = {};
					scope.focus = {};
					scope.keydown = function(index, event) {
						if (event.keyCode === 8) { // backspace
							scope.text = {};
							scope.focus[0] = true;
						} else if (event.keyCode === 37) { // left
							scope.focus[index - 1] = true;
						} else if (event.keyCode === 39) { // right
							scope.focus[index + 1] = true;
						}
					}

					ngModel.$render = function() {
						for (var k = 0; k < scope.length; k++) {
							if (ngModel.$viewValue) {
								scope.text[k] = ngModel.$viewValue[k];
							}
						}
					};

					var getText = function() {
						var t = "";
						for (var i = 0; i < scope.length; i++) {
							if (scope.text[i]) {
								t += scope.text[i][0];
							}
						}
						return t;
					};

					for (var j = 0; j < scope.length; ++j) {
						(function(j) {
							scope.$watch("text[" + j + "]", function(newValue, oldValue) {
								var fullText = getText();
								ngModel.$setViewValue(fullText);
								if (newValue && (!oldValue || newValue.length >= oldValue.length))
									scope.focus[j + 1] = true;
							});
						})(j);
					}
				}
			};
		}
	]);
})();