
<?php
try {
    $conn = new PDO('mysql:host=uwinfo344.chunkaiw.com;dbname=info344mysqlpdo', 'info344mysqlpdo', 'chrispaul');
    $stmt = $conn->prepare('SELECT name FROM Books');
    $stmt->execute();
    $result = $stmt->fetchAll();
    foreach ($result as $row) {
      echo($row['name']);
      echo "<br>";

    }

} catch(PDOException $e) {
  echo 'Error';
}
?>
