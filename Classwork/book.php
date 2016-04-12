<?php

class Book {
  private $name;
  private $price;

  function Book($name, $price) {
    $this->name = $name;
    $this->price = $price;
  }

  public function getName() {
    return $name;
  }

  public function getPrice() {
    return $price;
  }
}

public function getDefaultBooks() {
  return array(new Book("The Road", 9.99), new Book("1984", 5.75), new Book("The Hobbit", 12.00), new Book("Harry Potter and the Sorcerer's Stone", 17.65), new Book("Star Wars", 10.00));
}

?>
