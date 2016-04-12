<?php

if(isset($_POST['value'])) {
   $value = "%" . $_POST['value'] . "%";

   try {
       $conn = new PDO('mysql:host=nba.cdxfydz9vy1z.us-west-2.rds.amazonaws.com;port:3306;dbname=NBA', 'colin', 'feather!');
       $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
       $stmt = $conn->prepare("SELECT * FROM NBA.Stats WHERE Name LIKE :value");
       $stmt->bindValue(':value', $value);
       $stmt->execute();
       $result = $stmt->fetchAll(PDO::FETCH_ASSOC);
       echo json_encode($result);


   } catch(PDOException $e) {
     echo 'Error:' . $e;
   }
}

?>
