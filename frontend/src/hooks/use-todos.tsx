"use client";

import { useState } from "react";

export type Todo = {
  id: string;
  text: string;
  isActive: boolean;
};

export default function useTodos() {
  const [todos, setTodos] = useState<Todo[]>([]);

  const addTodos = (newValue: string): void => {
    if (!newValue.trim()) return;

    setTodos([
      ...todos,
      { id: crypto.randomUUID(), text: newValue, isActive: false },
    ]);
  };

  const toggleTodo = (id: string): void => {
    setTodos(
      todos.map((x) => (x.id === id ? { ...x, isActive: !x.isActive } : x)),
    );
  };

  const deleteTodo = (id: string): void => {
    setTodos(todos.filter((x) => x.id !== id));
  };

  return {
    todos,
    addTodos,
    toggleTodo,
    deleteTodo,
  };
}
