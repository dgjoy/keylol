(function() {
	"use strict";

	keylolApp.controller("LoginPasswordController", [
		"$scope", "close", "$http", "utils",
		function($scope, close, $http, utils) {
			$scope.vm = {
				EmailOrIdCode: "",
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
				if (!$scope.vm.EmailOrIdCode) {
					$scope.error["vm.EmailOrIdCode"] = "Email or UIC cannot be empty.";
				}
				if (!$scope.vm.Password) {
					$scope.error["vm.Password"] = "Password cannot be empty.";
				}
				if (typeof geetestResult === "undefined") {
					$scope.error.authCode = true;
				}
				if (!$.isEmptyObject($scope.error))
					return;
				$http.post("/api/login", $scope.vm)
					.then(function(response) {
						alert("登录成功");
					}, function(response) {
						switch (response.status) {
						case 400:
							$scope.error = response.data.ModelState;
							if ($scope.error.authCode) {
								gee.refresh();
							}
							break;
						default:
							console.error(response.data);
						}
					});
			};
		}
	]);
})();