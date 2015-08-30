(function() {
    "use strict";

    keylolApp.controller("PromotedReadingsController", [
        "$scope",
		function($scope) {
            $scope.recommendedArticles = [
                {
                    title: "Crusader Kings II 王国风云2",
                    summary: "大名鼎鼎的杀妻风云、娶女之王、推妹十字军、孩子养成计划。由于经常更新出新内容坑钱，导致汉化经常跟不上游戏版本。",
                    url: "test",
					datetime: moment().subtract(56, "seconds"),
                    recommendedByKeylol: true,
                    likeCount: 666,
                    commentCount: 222
                },
                {
                    title: "爷爷奶奶玩游戏 之 玩具熊的五夜",
                    summary: "本次爷爷奶奶试玩了玩具熊的五夜惊魂，当他们遇上恐怖游戏时，会发生什么呢？ 本视频由枫崎听译，如有疏漏欢迎指正。QQ群：305967685 欢迎各位朋友加入 视频制作同好更加欢迎 欢迎转帖，也请告诉本人一声",
                    url: "test",
					datetime: moment().subtract(88, "minutes"),
                    recommendedByKeylol: false,
                    likeCount: 666,
                    commentCount: 222
                },
                {
                    title: "德军总部：新秩序 单人剧情视频",
                    summary: "一开始的空中堡垒耐操度堪比神盾母舰=3=游戏画面还算精致，680开最好不是很卡好评。我都不知道一开始那个大家伙是可以KO的LOL",
                    url: "test",
					datetime: moment().subtract(26, "hours"),
                    recommendedByKeylol: false,
                    likeCount: 1337,
                    commentCount: 222
                },
                {
                    title: "枫崎原创翻译《战地：硬仗》彩蛋大合辑 Jack 哈哈哈哈哈哈哈哈哈哈哈哈哈哈哈",
                    summary: "油管著名播主Jackfrags日前发布了一则《硬仗》的彩蛋合辑短片，当中介绍了各种有意思的彩蛋。 本视频为枫崎听译，不免有所疏漏，欢迎指正。由于经常更新出新内容坑钱，导致汉化经常跟不上游戏版本。",
                    url: "test",
					datetime: moment().subtract(1, "years"),
                    recommendedByKeylol: false,
                    likeCount: 666,
                    commentCount: 222
                },
                {
                    title: "【极黑地牢】入门指导",
                    summary: "游戏本身我就不多加介绍了 roguelike+回合制RPG 另外有很多运营的成分在 目前处于开发阶段 地图开发了3/5 职业倒是差不多了 总之完成度不低了 正式版会有ios、PC、PS4好像",
                    url: "test",
					datetime: moment().subtract(6, "years"),
                    recommendedByKeylol: false,
                    likeCount: 666,
                    commentCount: 222
                }
            ];
        }
    ]);
})();