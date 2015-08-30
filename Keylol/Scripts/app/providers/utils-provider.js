﻿(function() {
	"use strict";

	keylolApp.provider("utils", function() {
		var _config = {};
		return {
			config: function(config) {
				_config = config;
			},
			$get: [
				function() {
					function Utils() {
						var self = this;
						var uniqueId = 1;

						self.byteLength = function(str) {
							var s = 0;
							for (var i = str.length - 1; i >= 0; i--) {
								var code = str.charCodeAt(i);
								if (code <= 0xff) s++;
								else if (code > 0xff && code <= 0xffff) s += 2;
								if (code >= 0xDC00 && code <= 0xDFFF) {
									i--;
									s++;
								} //trail surrogate
							}
							return s;
						};

						self.createGeetest = function(product, onSuccess) {
							if (typeof window.activateGeetest === "undefined") {
								window.activateGeetest = {};
							}
							var id = self.uniqueId();
							activateGeetest[id] = function() {
								var gee = new Geetest({
									gt: _config.geetestId,
									product: product,
									https: location.protocol === "https:"
								});
								gee.onSuccess(function() {
									onSuccess(gee.getValidate(), gee);
								});
								gee.appendTo("#geetest-" + id);
							};
							if (typeof window.Geetest === "undefined") {
								var s = document.createElement("script");
								s.src = "//api.geetest.com/get.php?callback=activateGeetest[" + id + "]";
								document.body.appendChild(s);
							} else {
								activateGeetest[id]();
							}
							return id;
						};

						self.uniqueId = function() {
							return uniqueId++;
						};

						self.modelValidate = {
							username: function(str, errorObj, modelName) {
								var usernameLength = self.byteLength(str);
								if (usernameLength < 3 || usernameLength > 16) {
									errorObj[modelName] = "UserName should be 3-16 bytes.";
									return false;
								}
								if (!/^[0-9A-Za-z\u4E00-\u9FCC]+$/.test(str)) {
									errorObj[modelName] = "Only digits, letters and Chinese characters are allowed in UserName.";
									return false;
								}
								return true;
							},
							password: function(str, errorObj, modelName) {
								if (str.length < 6) {
									errorObj[modelName] = "Passwords must be at least 6 characters.";
									return false;
								}
								return true;
							}
						};

						self.modelErrorDetect = {
							username: function(message) {
								if (/should.*bytes/.test(message))
									return "length";
								else if (/Only.*allowed/.test(message))
									return "format";
								else if (/already.*used/.test(message))
									return "used";
								return "unknown";
							},
							password: function(message) {
								if (/least.*characters/.test(message))
									return "length";
								else if (/not correct/.test(message))
									return "incorrect";
								else if (/cannot be empty/.test(message))
									return "empty";
								return "unknown";
							},
							email: function(message) {
								if (/already taken/.test(message))
									return "used";
								else if (/is invalid/.test(message))
									return "malformed";
								else if (/cannot be empty/.test(message))
									return "empty";
								else if (/doesn't exist/.test(message))
									return "inexistent";
								else if (/locked out/.test(message))
									return "lockedout";
								else if (/Login failed/.test(message))
									return "failed";
								return "unknown";
							}
						};
					}

					return new Utils();
				}
			]
		};
	});
})();