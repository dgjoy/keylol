(function() {
	"use strict";

	keylolApp.controller("LoginPasswordController", [
		"$scope", "close", "$http", "utils",
		function($scope, close, $http, utils) {
			$scope.vm = {
				Email: "",
				Password: ""
			};
			var geetestResult;
			var gee;
			$scope.geetestId = utils.createGeetest("float", function(result, geetest) {
				gee = geetest;
				geetestResult = result;
				$scope.vm.GeetestChallenge = geetestResult.geetest_challenge;
				$scope.vm.GeetestSeccode = geetestResult.geetest_seccode;
				$scope.vm.GeetestValidate = geetestResult.geetest_validate;
			});
			$scope.error = {};
			$scope.errorDetect = utils.modelErrorDetect;
			$scope.cancel = function() {
				close();
			};
			$scope.submit = function(form) {
				$scope.error = {};
				if (form.email.$invalid) {
					$scope.error["vm.Email"] = "is invalid";
				} else if (!$scope.vm.Email) {
					$scope.error["vm.Email"] = "empty";
				}
				if (!$scope.vm.Password) {
					$scope.error["vm.Password"] = "empty";
				}
				if (typeof geetestResult === "undefined") {
					$scope.error.authCode = true;
				}
				if (!$.isEmptyObject($scope.error))
					return;
//				$http.post("/api/user", $scope.vm)
//					.then(function(response) {
//						alert("注册成功");
//					}, function(response) {
//						$scope.error = response.data.ModelState;
//						if ($scope.error.authCode) {
//							gee.refresh();
//						}
//					});
			};
		}
	]);
})();