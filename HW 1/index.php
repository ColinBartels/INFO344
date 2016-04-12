<!DOCTYPE html>
<html>
   <head>
      <meta charset="UTF-8">
      <title>NBA Player Stats</title>
      <link href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.4/css/bootstrap.min.css" rel="stylesheet" type="text/css" />
      <script src="https://code.jquery.com/jquery.min.js"></script>
      <link rel="stylesheet" href="main.css"/>
      <script type="text/javascript" src="main.js"></script>
   </head>
   <body>
      <h1>NBA Player Stats</h1>

      <div class="form-group" id="searchdiv">
         <input type="text" placeholder="Search Player Name" class="form-control" id="searchbox" size="30">
      </div>

      <div id="table">
         <table id="tableHead" class="table table-striped table-hover table-condensed">
            <tr>
               <th></th>
               <th></th>
               <th></th>
               <th></th>
               <th>Field Goals</th>
               <th></th>
               <th></th>
               <th>3 Pointers</th>
               <th></th>
               <th></th>
               <th>Free Throws</th>
               <th></th>
               <th></th>
               <th>Rebounds</th>
               <th></th>
               <th></th>
               <th>Other</th>
               <th></th>
               <th></th>
               <th></th>
               <th></th>
               <th></th>
            </tr>
            <tr>
               <th>Name</th>
               <th>Team</th>
               <th>Games Played</th>
               <th>Minutes</th>
               <th>Made</th>
               <th>Attempted</th>
               <th>Percentage</th>
               <th>Made</th>
               <th>Attempted</th>
               <th>Percentage</th>
               <th>Made</th>
               <th>Attempted</th>
               <th>Percentage</th>
               <th>Offensive</th>
               <th>Defensive</th>
               <th>Total</th>
               <th>Assists</th>
               <th>Turnovers</th>
               <th>Steals</th>
               <th>Blocks</th>
               <th>Fouls</th>
               <th>Points Per Game</th>
            </tr>
            <?php dbQuery('SELECT * FROM NBA.Stats'); ?>
         </table>
      </div>
</html>

<?php
require_once('search.php');

function dbQuery($query) {
   try {
       $conn = new PDO('mysql:host=nba.cdxfydz9vy1z.us-west-2.rds.amazonaws.com;port:3306;dbname=NBA', 'colin', 'feather!');
       $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
       $stmt = $conn->prepare($query);
       $stmt->execute();
       $result = $stmt->fetchAll(PDO::FETCH_ASSOC);
       displayResults(json_encode($result));


   } catch(PDOException $e) {
     echo 'Error:' . $e;
   }
}

function displayResults($result) {
   $result = json_decode($result);

   foreach($result as $row) {
      echo '<tr class="remove">';
      echo '<td class="remove"><a href="http://en.wikipedia.org/wiki/' . $row->Name . '">' . $row->Name . "</a></td>";
      echo '<td class="remove">' . $row->Team . "</td>";
      echo '<td class="remove">' . $row->{'Games Played'} . "</td>";
      echo '<td class="remove">' . $row->Minutes . "</td>";
      echo '<td class="remove">' . $row->{'Field Goals Made'} . "</td>";
      echo '<td class="remove">' . $row->{'Field Goals Attempted'} . "</td>";
      echo '<td class="remove">' . $row->{'Field Goals Percentage'} . "</td>";
      echo '<td class="remove">' . $row->{'3 Points Made'} . "</td>";
      echo '<td class="remove">' . $row->{'3 Points Attempted'} . "</td>";
      echo '<td class="remove">' . $row->{'3 Points Percentage'} . "</td>";
      echo '<td class="remove">' . $row->{'Free Throws Made'} . "</td>";
      echo '<td class="remove">' . $row->{'Free Throws Attempted'} . "</td>";
      echo '<td class="remove">' . $row->{'Free Throws Percentage'} . "</td>";
      echo '<td class="remove">' . $row->{'Offensive Rebounds'} . "</td>";
      echo '<td class="remove">' . $row->{'Defensive Rebounds'} . "</td>";
      echo '<td class="remove">' . $row->{'Total Rebounds'} . "</td>";
      echo '<td class="remove">' . $row->Assists . "</td>";
      echo '<td class="remove">' . $row->Turnovers . "</td>";
      echo '<td class="remove">' . $row->Steals . "</td>";
      echo '<td class="remove">' . $row->Blocks . "</td>";
      echo '<td class="remove">' . $row->{'Personal Fouls'} . "</td>";
      echo '<td class="remove">' . $row->{'Points Per Game'} . "</td>";
      echo "</tr>";
   }
}

?>
