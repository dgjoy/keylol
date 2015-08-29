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
				//content: "<div>在这个风和日丽、喜大普奔的周五，我们<b><i><u>愉快地宣布</u></i></b>：</div><blockquote>UPYUN 云存储从 9 月 1 日开始正式免费啦！</blockquote><div>......</div><div>欲知具体，请 <u>传送门</u></div><div><a href=\"https://console.upyun.com/#/login/\">https://console.upyun.com/#/login/</a></div><div>上张图：</div><div><img src=\"http://lock522.b0.upaiyun.com/%E6%B8%85%E6%99%B0%E7%89%88.jpg\"></div><div><br></div><div>[文末福利]</div><div>今天开始，截止到 <b>8 月 31 日 正午 12 点</b>，评论本帖子即有机会获得 <b>《三体·死神永生》</b>（共 15 本）</div><h1><b>规则</b></h1><div>取本帖至活动截止时间总楼数的个位数为<b>“幸运数字”</b>，所有以幸运数字为个位数的评论楼层都将获奖（比如截止时达到 146 楼，所有楼层尾数为 6 的评论者都将获奖），如获奖者数目超过 15 ，将按以下优先顺序赠送奖品————幸运数字整数倍楼层＞偶数楼层＞奇数楼层</div><div>......</div><div>欢迎评论和交流（谨慎欢迎吐槽，但希望适度哈~）</div><div>欢迎评论和交流</div><div>欢迎评论和交流</div><div>欢迎评论和交流</div><div>再说三遍，撤...</div><div><br></div><blockquote>就这次免费，<b>如有问题咨询请关注我们的微信公众号，发信息给我们</b>，我们会尽可能快地解答。主要是因为在评论里留言容易被淹没掉~~~望谅解</blockquote><blockquote><img src=\"http://lock522.b0.upaiyun.com/%E5%85%AC%E4%BC%97%E5%8F%B7%E4%BA%8C%E7%BB%B4%E7%A0%81.jpg\"></blockquote>"
				content: "<h1>前言</h1><div>　　致力于给每个玩过的游戏写评测这就是我的一个<b>无聊的目标</b>... 由于我俩作都是玩得PS3版，这里就不贴评测页面了...</div><h1>最终幻想13</h1><div><img src=\"http://keylol.b0.upaiyun.com/nIq7Tvu.jpg\"></div><div>　　FF13，当初期待很久的游戏，首先，逼着我在PS3都快要换代的年份买了一台ps3的契机一半源于它，毕竟我也算是个FF的死忠（不算资深，因为接触的比较晚）。 <b><u>只不过没想到</u></b>，我刚刚没多久白金，SE居然一路把13扔到了PC上，从当初7、8交给EDIOS的劣质移植，到后来除了网游FF其他一概都不敢出在PC上的SE，再到吃掉了EDIOS以后，掌握了一定的PC技术胆肥了许多的SE，现在大举移植PC游戏也是稀松跟平常，不过这基本上已经凉透的冷饭......</div><div>　　好吧，也算是给PC玩家一个不用花太多钱买设备的理...... 干什么啊这是，你把FF14「最终幻想14」卖给盛大就啥都不玩了吗（而且好像还和这个没啥关系），因为SE有SE中囯在？半死的SE中囯运营着一个奄奄一息的幻想大陆，其他的还有么，真当天朝玩家不玩FF...... 结果居然被SE的跳票给玩死了，昔日的agito versus 被分别回锅成了零式和15，也就代表着原水晶传说计划被改的<i><u>面目全非</u></i>...... <a href=\"http://unicode-table.com/en/7EF4/\">http://unicode-table.com/en/7EF4/</a> FF13只能说是整个FF13故事线的开头。</div><blockquote>　　FF13，当初期待很久的游戏，首先，逼着我在PS3都快要换代的年份买了一台ps3的契机一半源于它，毕竟我也算是个FF的死忠（不算资深，因为接触的比较晚）。</blockquote><div>　　FF13继承了很多“后FF10时代”的问题——大段剧情看戏，一本道，没有大地图，结局超快通关后要素超磨蹭，并且比起FF10的时代还有所加强，其实这么一看也许是因为当初设定过于精细的画面反而到限制了细节的展开，导致没法做太多的支线。看的还是SE丧心病狂的锁区，FF13是官方中文游戏。居然把大陆锁了运行，这还叫亚洲版么。看似全都是即时演算的过场，但是到后来一看实际上有些是无缝连接的伪即时演。 应该说目前为止，除非是想要练级，实在是没见过为了通关都要刷的这么嗨的一款FF，可以说实在是扫兴，当然，不算上刷的时间的话，全成就/白金奖杯很容易，没有什么靠技术看运气的。在刷的过程中，你一定会永远地记住一个怪物精钢龟......没错，通关后这就是你唯一拿来练级和赚钱的怪物了......</div><h1>总结</h1><div>　　SE的“榨干”论之下，FF越来越像是一个手游了——<i>该简化的不简化</i>，<u>不该简化的都简化了</u>，用抓狂的要求和拖延的设定增加游戏黏度而不是用一些可以反复游玩的要素，这些都是FF目前的问题。FF还能不能抓住我这个标准的“FF迷”呢，恐怕什么时候我不是硬着头皮玩FF，也许FF就能够回归之前的感动了</div>"
			};
		}
	]);
})();