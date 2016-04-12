$(function() {
   $("#searchbox").on('keyup', function() {
      var value = $('#searchbox').val();
      $.post('search.php', {value:value}, function(data) {
         $(".remove").remove();

         $.each($.parseJSON(data), function(i, item) {
            var tr = $('<tr class="remove">').append(
               $('<td class="remove">').append($('<a>' + item["Name"] + '</a>').attr("href", 'http://en.wikipedia.org/wiki/' + item["Name"])),
               $('<td class="remove">').text(item["Team"]),
               $('<td class="remove">').text(item["Games Played"]),
               $('<td class="remove">').text(item["Minutes"]),
               $('<td class="remove">').text(item["Field Goals Made"]),
               $('<td class="remove">').text(item["Field Goals Attempted"]),
               $('<td class="remove">').text(item["Field Goals Percentage"]),
               $('<td class="remove">').text(item["3 Points Made"]),
               $('<td class="remove">').text(item["3 Points Attempted"]),
               $('<td class="remove">').text(item["3 Points Percentage"]),
               $('<td class="remove">').text(item["Free Throws Made"]),
               $('<td class="remove">').text(item["Free Throws Attempted"]),
               $('<td class="remove">').text(item["Free Throws Percentage"]),
               $('<td class="remove">').text(item["Offensive Rebounds"]),
               $('<td class="remove">').text(item["Defensive Rebounds"]),
               $('<td class="remove">').text(item["Total Rebounds"]),
               $('<td class="remove">').text(item["Total Rebounds"]),
               $('<td class="remove">').text(item["Turnovers"]),
               $('<td class="remove">').text(item["Steals"]),
               $('<td class="remove">').text(item["Blocks"]),
               $('<td class="remove">').text(item["Personal Fouls"]),
               $('<td class="remove">').text(item["Points Per Game"])
            );
            tr.appendTo('#tableHead');
         });
      });
   return false;
   });
});
