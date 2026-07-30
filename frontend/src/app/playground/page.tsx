"use client";

import { Button } from "@/components/ui/button";
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
      { id: crypto.randomUUID(), text: newValue, isActive: true },
    ]);
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
      </div>
      <ul>
        {todos.map((x) => (
          <li key={x.id}>{x.text}</li>
        ))}
      </ul>
    </div>
  );
}
