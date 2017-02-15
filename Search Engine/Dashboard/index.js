$(document).ready(function () {
    $("#searchbox").on("keyup", function () {
        var list = $("#suggestions");
        var input = $("#searchbox").val();
        input = input.toLowerCase();
        if (input.length > 0) {
            input = input.replace(" ", "_");
            var val = {
                prefix: input
            };
            $.ajax({
                type: "POST",
                url: "/queryservice.asmx/findSuggestions",
                data: JSON.stringify(val),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (data) {
                    data = data.d;
                    data = data.replace("[", "");
                    data = data.replace("]", "");
                    data = data.replace(/\"/g, "");
                    data = data.replace(/_/g, " ");
                    data = data.split(",");
                    list.empty();
                    $.each(data, function (i, item) {
                        list.append($("<option>").attr("value", item));
                    });
                }
            });
        } else {
            list.empty();
        }
    });

    $("#submit").click(function () {
        $.when(getSearchResults(), getPlayerStats()).done(function (results, stats) {
            var div = $("#stats");
            div.empty();
            data = stats[0][0];
            if (data != null) {
                div.append($("<h3>" + data.Name + "</h3>"));
                div.append($("<p>Team: </p><span>" + data.Team + "</span>"));
                div.append($("<p>Games Played: </p><span>" + data["Games Played"] + "</span>"));
                div.append($("<p>Minutes: </p><span>" + data.Minutes + "</p>"));
                div.append($("<p>FG Percentage: </p><span>" + data["Field Goals Percentage"] + "</span>"));
                div.append($("<p>3 Point Percentage: </p><span>" + data["3 Points Percentage"] + "</span>"));
                div.append($("<p>Free Throws Percentage: </p><span>" + data["Free Throws Percentage"] + "</span>"));
                div.append($("<p>Offensive Rebounds: </p><span>" + data["Offensive Rebounds"] + "</span>"));
                div.append($("<p>Defensive Rebounds: </p><span>" + data["Defensive Rebounds"] + "</span>"));
                div.append($("<p>Turnovers: </p><span>" + data["Turnovers"] + "</span>"));
                div.append($("<p>Steals: </p><span>" + data["Steals"] + "</span>"));
                div.append($("<p>Blocks: </p><span>" + data["Blocks"] + "</span>"));
                div.append($("<p>Fouls: </p><span>" + data["Personal Fouls"] + "</span>"));
                div.append($("<p>Points Per Game: </p><span>" + data["Points Per Game"] + "</span>"));
            }

            var searchResults = JSON.parse(results[0].d);
            var list = $("#resultsList");
            list.empty();
            if (searchResults == "No Results") {
                var li = $("<li>No Results</li>");
                list.append(li);
            } else {
                for (var i = 0; i < searchResults.length; i++) {
                    var li = $("<li><a href='" + searchResults[i].Url + "'>" + searchResults[i].Title + "</a></li>");
                    list.append(li);
                }
            }
            $("#submit").removeAttr("disabled");
        });
    });
});

function getSearchResults() {
    $("#submit").attr("disabled", "disabled");
    var input = $("#searchbox").val();
    if (input.length > 0) {
        var val = {
            input: input
        };
        console.log(input);
        return $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/CrawlerService.asmx/Search",
            data: JSON.stringify(val),
            dataType: "json"
        });
    }
}

function getPlayerStats() {
    return $.getJSON('http://ec2-52-35-44-108.us-west-2.compute.amazonaws.com/jsonp.php?callback=?', 'value=' + $("#searchbox").val());
}