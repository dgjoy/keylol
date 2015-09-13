(function() {
    "use strict";

    keylolApp.controller("SettingsController", [
        "$scope", "close", "utils", "$http", "union",
        function($scope, close, utils, $http, union) {
            $scope.error = {};
            $scope.errorDetect = utils.modelErrorDetect;
            $scope.page = "profiles";
            $scope.uniqueIds = {};
            for (var i = 0; i < 22; ++i) {
                $scope.uniqueIds[i] = utils.uniqueId();
            }
            $scope.union = union;

            $scope.vm = {
                ProfilePointBackgroundImage: "",
                Password: "",
                NewPassword: "",
                ConfirmPassword: ""
            };
            var originalVM = {};
            var updateVM = function(user) {
                $.extend($scope.vm, {
                    GamerTag: user.GamerTag,
                    AvatarImage: user.AvatarImage,
                    Email: user.Email,

                    AutoShareOnAcquiringNewGame: user.AutoShareOnAcquiringNewGame,
                    AutoShareOnAddingFavorite: user.AutoShareOnAddingFavorite,
                    AutoShareOnAddingNewFriend: user.AutoShareOnAddingNewFriend,
                    AutoShareOnAddingVideo: user.AutoShareOnAddingVideo,
                    AutoShareOnCreatingGroup: user.AutoShareOnCreatingGroup,
                    AutoShareOnJoiningGroup: user.AutoShareOnJoiningGroup,
                    AutoShareOnPublishingReview: user.AutoShareOnPublishingReview,
                    AutoShareOnUnlockingAchievement: user.AutoShareOnUnlockingAchievement,
                    AutoShareOnUpdatingWishlist: user.AutoShareOnUpdatingWishlist,
                    AutoShareOnUploadingScreenshot: user.AutoShareOnUploadingScreenshot,

                    LockoutEnabled: user.LockoutEnabled,

                    EmailNotifyOnAdvertisement: user.EmailNotifyOnAdvertisement,
                    EmailNotifyOnArticleReplied: user.EmailNotifyOnArticleReplied,
                    EmailNotifyOnCommentReplied: user.EmailNotifyOnCommentReplied,
                    EmailNotifyOnEditorRecommended: user.EmailNotifyOnEditorRecommended,
                    EmailNotifyOnMessageReceived: user.EmailNotifyOnMessageReceived,

                    MessageNotifyOnArticleLiked: user.MessageNotifyOnArticleLiked,
                    MessageNotifyOnArticleReplied: user.MessageNotifyOnArticleReplied,
                    MessageNotifyOnCommentLiked: user.MessageNotifyOnCommentLiked,
                    MessageNotifyOnCommentReplied: user.MessageNotifyOnCommentReplied,
                    MessageNotifyOnEditorRecommended: user.MessageNotifyOnEditorRecommended
                });
                $.extend(originalVM, $scope.vm);
            };
            var isVMDirty = function(key) {
                return $scope.vm[key] !== originalVM[key];
            };
            updateVM(union.$localStorage.user);

            $http.get("api/user/" + union.$localStorage.login.UserId + "?includeClaims=true&includeProfilePointBackgroundImage=true").then(function(response) {
                var user = response.data;
                $scope.vm.ProfilePointBackgroundImage = user.ProfilePointBackgroundImage;
                delete user.ProfilePointBackgroundImage;
                updateVM(user);
                union.$localStorage.user = user;
            });

            var geetestResult;
            var gee;
            $scope.geetestId = utils.createGeetest("float", function(result, geetest) {
                gee = geetest;
                geetestResult = result;
                $scope.vm.GeetestChallenge = geetestResult.geetest_challenge;
                $scope.vm.GeetestSeccode = geetestResult.geetest_seccode;
                $scope.vm.GeetestValidate = geetestResult.geetest_validate;
            });

            $scope.cancel = function() {
                close();
            };

            $scope.setPage = function(page) {
                $scope.page = page;
            };

            $scope.optionsInPageChanged = function(page) {
                var keys = [];
                switch (page) {
                case "profiles":
                    keys = [
                        "GamerTag",
                        "Email",
                        "AvatarImage",
                        "ProfilePointBackgroundImage"
                    ];
                    break;
                case "platform":
                    keys = [
                        "AutoShareOnAcquiringNewGame",
                        "AutoShareOnAddingFavorite",
                        "AutoShareOnAddingNewFriend",
                        "AutoShareOnAddingVideo",
                        "AutoShareOnCreatingGroup",
                        "AutoShareOnJoiningGroup",
                        "AutoShareOnPublishingReview",
                        "AutoShareOnUnlockingAchievement",
                        "AutoShareOnUpdatingWishlist",
                        "AutoShareOnUploadingScreenshot"
                    ];
                    break;
                case "security":
                    keys = [
                        "LockoutEnabled",
                        "EmailNotifyOnAdvertisement",
                        "EmailNotifyOnArticleReplied",
                        "EmailNotifyOnCommentReplied",
                        "EmailNotifyOnEditorRecommended",
                        "EmailNotifyOnMessageReceived",
                        "NewPassword"
                    ];
                    break;
                case "preferences":
                    keys = [
                        "EmailNotifyOnAdvertisement",
                        "EmailNotifyOnArticleReplied",
                        "EmailNotifyOnCommentReplied",
                        "EmailNotifyOnEditorRecommended",
                        "EmailNotifyOnMessageReceived",
                        "MessageNotifyOnArticleLiked",
                        "MessageNotifyOnArticleReplied",
                        "MessageNotifyOnCommentLiked",
                        "MessageNotifyOnCommentReplied",
                        "MessageNotifyOnEditorRecommended"
                    ];
                    break;
                }
                return keys.some(function(key) {
                    return isVMDirty(key);
                });
            };

            $scope.optionsInPageError = function(page) {
                var keys = [];
                switch (page) {
                case "profiles":
                    keys = [
                        "vm.GamerTag",
                        "vm.Email"
                    ];
                    break;
                case "platform":
                    break;
                case "security":
                    keys = [
                        "vm.Password",
                        "vm.NewPassword",
                        "vm.ConfirmPassword",
                        "authCode"
                    ];
                    break;
                case "preferences":
                    break;
                }
                return keys.some(function(key) {
                    return $scope.error[key];
                });
            };

            var focusErrorPage = function() {
                var pages = ["profiles", "platform", "security", "preferences"];
                for (var pi in pages) {
                    if (pages.hasOwnProperty(pi) && $scope.optionsInPageError(pages[pi])) {
                        $scope.page = pages[pi];
                        break;
                    }
                }
            };

            $scope.submit = function() {
                $scope.error = {};
                utils.modelValidate.gamerTag($scope.vm.GamerTag, $scope.error, "vm.GamerTag");
                if (isVMDirty("NewPassword") || isVMDirty("LockoutEnabled")) {
                    if (!$scope.vm.Password) {
                        $scope.error["vm.Password"] = "Password cannot be empty.";
                    }
                    if (typeof geetestResult === "undefined") {
                        $scope.error.authCode = true;
                    }
                    if ($scope.vm.NewPassword) {
                        if (utils.modelValidate.password($scope.vm.NewPassword, $scope.error, "vm.NewPassword")) {
                            if ($scope.vm.NewPassword !== $scope.vm.ConfirmPassword) {
                                $scope.error["vm.ConfirmPassword"] = "not match";
                            }
                        }
                    }
                }
                if (isVMDirty("Email")) {
                    if (form.email.$invalid) {
                        $scope.error["vm.Email"] = "Email is invalid.";
                    } else if (!$scope.vm.Email) {
                        $scope.error["vm.Email"] = "Email cannot be empty.";
                    }
                }
                if (!$.isEmptyObject($scope.error)) {
                    focusErrorPage();
                    return;
                }

                // Only submit dirty fields
                var dirtyFields = {};
                for (var key in $scope.vm) {
                    if ($scope.vm.hasOwnProperty(key) && isVMDirty(key)) {
                        dirtyFields[key] = $scope.vm[key];
                    }
                }
                if ($.isEmptyObject(dirtyFields)) { // Nothing changed
                    alert("保存成功。");
                    close();
                    return;
                }

                $http.put("api/user/" + union.$localStorage.login.UserId, dirtyFields)
                    .then(function() {
                        $.extend(union.$localStorage.user, dirtyFields);
                        alert("保存成功。");
                        close();
                    }, function(response) {
                        switch (response.status) {
                        case 400:
                            $scope.error = response.data.ModelState;
                            if ($scope.error.authCode) {
                                gee.refresh();
                            }
                            focusErrorPage();
                            break;
                        default:
                            alert(response.data);
                        }
                    });
            };

            $scope.logout = function() {
                if (confirm("确认退出登录？")) {
                    $http.delete("api/login/current").then(function() {
                        delete union.$localStorage.login;
                        alert("成功退出登录。");
                        close();
                    }, function(response) {
                        alert(response.data);
                    });
                }
            };
        }
    ]);
})();