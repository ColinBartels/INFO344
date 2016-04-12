<?php

require_once('book.php');

$books = getDefaultBooks();
for ($i=0; $i < count($books); $i++) {
  echo "Name: " . $books[$i]->getName() . " Price: " . $books[$i]->getPrice() . "\n";
}

 ?>
