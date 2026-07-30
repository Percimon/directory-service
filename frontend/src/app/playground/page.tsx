"use client";

import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { useState } from "react";

type Todo = {
  id: string;
  text: string;
  isActive: boolean;
};

export default function PlaygroundPage() {
  const [newValue, setTodoValue] = useState<string>("");

  const [todos, setTodos] = useState<Todo[]>([]);

  function addTodos() {
    if (!newValue.trim()) return;

    setTodos([
      ...todos,
      { id: crypto.randomUUID(), text: newValue, isActive: false },
    ]);
  }

  function toggleTodo(id: string) {
    setTodos(
      todos.map((x) => (x.id === id ? { ...x, isActive: !x.isActive } : x)),
    );
  }

  function deleteTodo(id: string) {
    setTodos(todos.filter((x) => x.id !== id));
  }

  return (
    <div className="flex flex-col">
      <div className="grid grid-cols-2 gap-5 justify-center">
        <Input
          value={newValue}
          placeholder="Введите значение.."
          onChange={(e) => setTodoValue(e.currentTarget.value)}
        />
        <Button onClick={() => addTodos()}>Create</Button>
        <p>Невыполнено: {todos.filter((x) => x.isActive === false).length}</p>
      </div>
      <ul>
        {todos.map((x) => (
          <li key={x.id}>
            <div className="flex flex-row gap-3">
              <Checkbox checked={x.isActive} onClick={() => toggleTodo(x.id)} />
              {x.text}
              <Button onClick={() => deleteTodo(x.id)}>X</Button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
