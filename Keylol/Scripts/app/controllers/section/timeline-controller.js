(function() {
    "use strict";

    keylolApp.controller("TimelineController", [
        "$scope", function ($scope) {
            $scope.headingDisplayMode = function(piece) {
                if (piece.source)
                    return "source";
                else
                    return "title";
            };
            $scope.contentDisplayMode = function(piece) {
                if (piece.source) {
                    if (piece.thumbnails)
                        return "title-thumbnail";
                    else
                        return "title-summary";
                } else {
                    if (piece.thumbnails)
                        return "thumbnails";
                    else
                        return "summary";
                }
            };
            $scope.pieces = [
                {
                    types: ["评测", "好评"],
                    source: {
                        name: "战地：硬仗",
                        url: "test"
                    },
                    author: {
                        username: "crakyGALU",
                        gamerTag: "怀旧CS玩家，保护1.6人人有责",
                        avatarUrl: "Content/Images/exit.png"
                    },
                    title: "代工就是不如原厂：这硬仗真的是打的艰辛",
                    summary: "说起这个警匪死磕的话，相比正玩家们没有不知道那大名鼎鼎的CF啊不对CS，也就是《反恐精英（Counter-Strike）》，这款游戏的出现让警匪对殴的模式风靡全球，至今依然有大批玩家热衷于此，除此之外类似的游戏也有不少，例如《彩虹六号系列》，例如《收获日系列》等等，如今FPS界极其有影响力的战地系列也开始涉及此类题材，眼下BETA版本已经开始测试，本篇评测是根据测试版而来，所以并不代表游戏的最终品质！",
                    url: "test",
                    thumbnails: [
                        "Content/Images/exit.png"
                    ],
                    repostCount: 522,
                    likeCount: 666,
                    commentCount: 222
                },
                {
                    types: ["模组"],
                    source: {
                        name: "战地：硬仗",
                        url: "test",
                        repost: true
                    },
                    author: {
                        username: "crakyGALU",
                        avatarUrl: "Content/Images/exit.png"
                    },
                    title: "代工就是不如原厂：这硬仗真的是打的艰辛 字数补丁字数补丁字数补丁字数补丁字数补丁字数补丁字数补丁",
                    summary: "说起这个警匪死磕的话，相比正玩家们没有不知道那大名鼎鼎的CF啊不对CS，也就是《反恐精英（Counter-Strike）》，这款游戏的出现让警匪对殴的模式风靡全球，至今依然有大批玩家热衷于此，除此之外类似的游戏也有不少，例如《彩虹六号系列》，例如《收获日系列》等等，如今FPS界极其有影响力的战地系列也开始涉及此类题材，眼下BETA版本已经开始测试，本篇评测是根据测试版而来，所以并不代表游戏的最终品质！",
                    url: "test",
                    repostCount: 522,
                    likeCount: 666,
                    commentCount: 222
                },
                {
                    types: ["资讯"],
                    author: {
                        username: "crakyGALU",
                        gamerTag: "怀旧CS玩家，保护1.6人人有责字数补丁字数补丁字数补丁",
                        avatarUrl: "Content/Images/exit.png"
                    },
                    title: "代工就是不如原厂：这硬仗真的是打的艰辛",
                    summary: "说起这个警匪死磕的话，相比",
                    url: "test",
                    thumbnails: [
                        "Content/Images/exit.png",
                        "Content/Images/cuc.png",
                        "Content/Images/mind-gap.png"
                    ],
                    repostCount: 522,
                    likeCount: 666,
                    commentCount: 222
                },
                {
                    types: ["评测", "差评"],
                    author: {
                        username: "crakyGALU字数补丁字数补丁字数补丁",
                        gamerTag: "怀旧CS玩家，保护1.6人人有责",
                        avatarUrl: "Content/Images/exit.png"
                    },
                    title: "代工就是不如原厂：这硬仗真的是打的艰辛 字数补丁字数补丁字数补丁字数补丁字数补丁字数补丁",
                    summary: "说起这个警匪死磕的话，相比正玩家们没有不知道那大名鼎鼎的CF啊不对CS，也就是《反恐精英（Counter-Strike）》",
                    url: "test",
                    repostCount: 522,
                    likeCount: 666,
                    commentCount: 222
                }
            ];
        }
    ]);

    keylolApp.controller("ThumbnailSwitchController", [
        "$scope", function($scope) {
            $scope.currentThumbnailIndex = 0;
            $scope.changeCurrentThumbnail = function(index) {
                $scope.currentThumbnailIndex = index;
            };
        }
    ]);
})();