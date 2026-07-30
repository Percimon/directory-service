"use client";

import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import useTodos from "@/hooks/use-todos";
import { useState } from "react";

export default function PlaygroundPage() {
  const [newValue, setTodoValue] = useState<string>("");

  const { todos, addTodos, toggleTodo, deleteTodo } = useTodos();

  return (
    <div className="flex flex-col">
      <div className="grid grid-cols-2 gap-5 justify-center">
        <Input
          value={newValue}
          placeholder="Введите значение.."
          onChange={(e) => setTodoValue(e.currentTarget.value)}
        />
        <Button onClick={() => addTodos(newValue)}>Create</Button>
        <p>Невыполнено: {todos.filter((x) => x.isActive === false).length}</p>
      </div>
      <ul>
        {todos.map((x) => (
          <li key={x.id}>
            <div className="flex flex-row gap-3">
              <Checkbox
                checked={x.isActive}
                onCheckedChange={() => toggleTodo(x.id)}
              />
              {x.text}
              <Button onClick={() => deleteTodo(x.id)}>X</Button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
