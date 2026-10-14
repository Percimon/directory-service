import { queryClient } from "@/shared/api/query-client";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
} from "@/shared/components/ui/dialog";
import { useQueryClient } from "@tanstack/react-query";

type CreateLocationFormProps = {
  open: boolean;
  onOpenChange: (open: true) => void;
};

export default function CreateLocationDialog({
  open,
  onOpenChange,
}: CreateLocationFormProps) {
  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent>
          <DialogHeader>Создание локации</DialogHeader>
          <DialogDescription>
            Заполните форму для создания локации
          </DialogDescription>
        </DialogContent>
      </Dialog>

      <form className="grid space-y-4">
        <div>
          <label htmlFor="name">Название</label>
          <input id="name" name="name" type="text" />
        </div>
        <div>
          <label htmlFor="city">Город</label>
          <input id="city" name="city" type="text" />
        </div>
        <div>
          <label htmlFor="district">Район</label>
          <input id="district" name="district" type="text" />
        </div>
        <div>
          <label htmlFor="street">Улица</label>
          <input id="street" name="street" type="text" />
        </div>
        <div>
          <label htmlFor="structure">Строение</label>
          <input id="structure" name="structure" type="text" />
        </div>
        <div>
          <label htmlFor="timeZone">Часовой пояс</label>
          <input id="timeZone" name="timeZone" type="text" />
        </div>
        <button type="submit">Создать</button>
      </form>
    </>
  );
}
