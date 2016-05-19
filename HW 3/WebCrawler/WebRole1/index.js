google.charts.load('current', { 'packages': ['gauge'] });
google.charts.setOnLoadCallback(drawChart);
function drawChart() {

    var data = google.visualization.arrayToDataTable([
      ['Label', 'Value'],
      ['Memory', 0],
      ['CPU', 0]
    ]);

    var options = {
        width: 400, height: 120,
        redFrom: 90, redTo: 100,
        yellowFrom: 75, yellowTo: 90,
        minorTicks: 5
    };

    function update() {
        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/CrawlerService.asmx/GetSystemInfo",
            data: "{}",
            dataType: "json",
            success: function (result) {
                result = JSON.stringify(result);
                result = result.replace(/"/g, "")
                .replace(/\\/g, "")
                .replace(/{/g, "")
                .replace(/}/g, "")
                .split(":");
                data.setValue(0, 1, result[1]);
                data.setValue(1, 1, result[2]);
                chart.draw(data, options);
            }
        });
    }

    var chart = new google.visualization.Gauge(document.getElementById('chart_div'));

    update();
    setInterval(update, 3000);
}

function getLast10() {
    return $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/CrawlerService.asmx/GetLast10",
        data: "{}",
        dataType: "json"
    });
}

function getSizes() {
    return $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/CrawlerService.asmx/GetSize",
        data: "{}",
        dataType: "json"
    });
}

function getErrors() {
    return $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/CrawlerService.asmx/GetErrors",
        data: "{}",
        dataType: "json"
    });
}

function getStatus() {
    return $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "/CrawlerService.asmx/GetStatus",
        data: "{}",
        dataType: "json"
    });
}

function findTitle() {
    var url = $("#search").val();
    if (url.length > 0) {
        url = url.toLowerCase();
        var val = {
            url: url
        };
        return $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/CrawlerService.asmx/GetPageTitle",
            data: JSON.stringify(val),
            dataType: "json",
            success: function (result) {
                result = result.d
                .replace(/"/g, "");
                $("#titleResult").html("Title: " + result);
            }
        });
    }
}

function updateData() {
    $.when(getSizes(), getLast10(), getErrors(), getStatus()).done(function (sizes, last10, errors, status) {
        var sizesResponse = sizes[0].d.replace(/"/g, "");
        var sizesSplit = sizesResponse.split(":");
        $("#urls").text(sizesSplit[0]);
        $("#queue").text(sizesSplit[1]);
        $("#index").text(sizesSplit[2]);

        $("#list").empty();
        var last10Response = last10[0].d.replace(/"/g, "");
        var last10List = last10Response.split("|");
        for (var i = 0; i < last10List.length; i++) {
            var li = $("<li>" + last10List[i] + "</li>");
            $("#list").append(li);
        }

        $("#errorsList").empty();
        var errorsResponse = JSON.parse(errors[0].d);
        for (var i = 0; i < errorsResponse.length; i++) {
            var li = $("<li>" + "Error: " + errorsResponse[i].error + "<br>URL: " + errorsResponse[i].url + "</li>");
            $("#errorsList").append(li);
        }

        var statusResponse = status[0].d.replace(/"/g, "");
        $("#status").text(statusResponse);
        if (statusResponse == "idle") {
            $("#start").removeAttr("disabled");
            $("#pause").attr("disabled", "disabled");
        }
        else if (statusResponse == "loading" || statusResponse == "crawling") {
            $("#start").attr("disabled", "disabled");
            $("#pause").removeAttr("disabled");
        }
    });
}

$(document).ready(function () {
    updateData();

    $("#start").click(function () {
        $.get("/CrawlerService.asmx/StartCrawling", function () {
            $("#start").attr("disabled", "disabled");
        });
    });
    $("#pause").click(function () {
        $.get("/CrawlerService.asmx/StopCrawling", function () {
            $("#pause").attr("disabled", "disabled");
        });
    });
    $("#clear").click(function () {
        $.get("/CrawlerService.asmx/ClearAll", function () {
            $("#pause").attr("disabled", "disabled");
            $("#clear").attr("disabled", "disabled");
            $("#update").attr("disabled", "disabled");
        });
    });
    $("#submit").click(function () {
        findTitle();
    });
    $("#update").click(function () {
        updateData();
    });
});