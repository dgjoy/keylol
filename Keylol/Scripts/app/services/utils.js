(function() {
	"use strict";

	keylolApp.factory("utils", [
		function() {
			return {
				byteLength: function(str) {
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
				},
				createGeetest: function(product, onSuccess) {
					if (typeof window.activateGeetest === "undefined") {
						window.activateGeetest = [];
					}
					var geetestId = activateGeetest.length;
					activateGeetest[geetestId] = function() {
						var gee = new Geetest({
							gt: "0c002064ef8f602ced7bccec08b8e10b",
							product: product,
							https: location.protocol === "https:"
						});
						gee.onSuccess(function() {
							onSuccess(gee.getValidate(), gee);
						});
						gee.appendTo("#geetest-" + geetestId);
					};
					if (typeof window.Geetest === "undefined") {
						var s = document.createElement("script");
						s.src = "//api.geetest.com/get.php?callback=activateGeetest[" + geetestId + "]";
						document.body.appendChild(s);
					} else {
						activateGeetest[geetestId]();
					}
					return geetestId;
				},
				modelErrorDetect: {
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
						return "unknown";
					},
					email: function(message) {
						if (/already taken/.test(message))
							return "used";
						else if (/is invalid/.test(message))
							return "malformed";
						else if (message === "empty")
							return "empty";
						return "unknown";
					}
				}
			};
		}
	]);
})();