(function() {
	"use strict";

	keylolApp.controller("EditorController", [
		"$scope", "close", "$element", "utils",
		function($scope, close, $element, utils) {
			$scope.cancel = function() {
				close();
			};
			$scope.radioId = [utils.uniqueId(), utils.uniqueId(), utils.uniqueId()];
			$scope.vm = {
				content: "<div>在这个风和日丽、喜大普奔的周五，我们<b><i><u>愉快地宣布</u></i></b>：</div><blockquote>UPYUN 云存储从 9 月 1 日开始正式免费啦！</blockquote><div>......</div><div>欲知具体，请 <u>传送门</u></div><div><a href=\"https://console.upyun.com/#/login/\">https://console.upyun.com/#/login/</a></div><div>上张图：</div><div><img src=\"http://lock522.b0.upaiyun.com/%E6%B8%85%E6%99%B0%E7%89%88.jpg\"></div><div><br></div><div>[文末福利]</div><div>今天开始，截止到 <b>8 月 31 日 正午 12 点</b>，评论本帖子即有机会获得 <b>《三体·死神永生》</b>（共 15 本）</div><h1><b>规则</b></h1><div>取本帖至活动截止时间总楼数的个位数为<b>“幸运数字”</b>，所有以幸运数字为个位数的评论楼层都将获奖（比如截止时达到 146 楼，所有楼层尾数为 6 的评论者都将获奖），如获奖者数目超过 15 ，将按以下优先顺序赠送奖品————幸运数字整数倍楼层＞偶数楼层＞奇数楼层</div><div>......</div><div>欢迎评论和交流（谨慎欢迎吐槽，但希望适度哈~）</div><div>欢迎评论和交流</div><div>欢迎评论和交流</div><div>欢迎评论和交流</div><div>再说三遍，撤...</div><div><br></div><blockquote>就这次免费，<b>如有问题咨询请关注我们的微信公众号，发信息给我们</b>，我们会尽可能快地解答。主要是因为在评论里留言容易被淹没掉~~~望谅解</blockquote><blockquote><img src=\"http://lock522.b0.upaiyun.com/%E5%85%AC%E4%BC%97%E5%8F%B7%E4%BA%8C%E7%BB%B4%E7%A0%81.jpg\"></blockquote>"
			};
		}
	]);
})();