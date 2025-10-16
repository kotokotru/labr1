using System;
using System.Collections.Generic;

public enum CarStatus
{
    Waiting,
    Repairing,
    Fixed,
    CannotFix
}

public record CarRecord(string VIN, string Model);

public class CannotRepairCarException : Exception
{
    private readonly string message;
    public override string Message => message;

    public CannotRepairCarException(string message)
    {
        this.message = message;
    }
}

public abstract class Person
{
    public string Name { get; private set; }

    protected Person(string name)
    {
        Name = name;
    }

    public abstract void Work();
}

public interface IRepair
{
    void Repair(WorkOrder order);
}

public class Car
{
    private CarRecord info;
    public CarRecord Info => info;

    public CarStatus Status { get; private set; }

    public Car(string vin, string model)
    {
        info = new CarRecord(vin, model);
        Status = CarStatus.Waiting;
    }

    internal void SetStatus(CarStatus status)
    {
        Status = status;
    }

    public override bool Equals(object obj)
    {
        if (obj is Car otherCar)
            return info.VIN == otherCar.info.VIN;

        return false;
    }

    public override int GetHashCode()
    {
        return info.VIN.GetHashCode();
    }

    public override string ToString()
    {
        return $"{info.Model} (VIN: {info.VIN}), Статус: {Status}";
    }
}

public class Client : Person
{
    public Client(string name) : base(name) { }

    public override void Work()
    {
        Console.WriteLine($"{Name} - клиент, ожидающий ремонта автомобиля.");
    }

    public WorkOrder CreateWorkOrder(Car car, Mechanic mechanic)
    {
        if (car.Status == CarStatus.CannotFix)
            throw new CannotRepairCarException($"Автомобиль {car.Info.Model} невозможно отремонтировать.");

        Console.WriteLine($"{Name} создал заказ на ремонт автомобиля {car.Info.Model}.");
        return new WorkOrder(this, car, mechanic);
    }
}

public class Mechanic : Person, IRepair
{
    private Random random = new Random();

    public Mechanic(string name) : base(name) { }

    public override void Work()
    {
        Console.WriteLine($"{Name} - механик, готов к работе.");
    }

    public void Repair(WorkOrder order)
    {
        Console.WriteLine($"{Name} начал ремонт автомобиля {order.Car.Info.Model}");
        order.Car.SetStatus(CarStatus.Repairing);

        int result = random.Next(100);
        if (result < 75)
        {
            order.Car.SetStatus(CarStatus.Fixed);
            Console.WriteLine($"{Name} успешно отремонтировал {order.Car.Info.Model}");
        }
        else
        {
            order.Car.SetStatus(CarStatus.CannotFix);
            Console.WriteLine($"{Name} сообщил, что {order.Car.Info.Model} не подлежит ремонту.");
            throw new CannotRepairCarException($"Автомобиль {order.Car.Info.Model} не подлежит ремонту.");
        }
    }
}

public class WorkOrder
{
    public Client Client { get; }
    public Car Car { get; }
    public Mechanic Mechanic { get; }

    public WorkOrder(Client client, Car car, Mechanic mechanic)
    {
        Client = client;
        Car = car;
        Mechanic = mechanic;
    }

    public void Process()
    {
        try
        {
            Mechanic.Repair(this);
        }
        catch (CannotRepairCarException ex)
        {
            Console.WriteLine($"Исключение: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"Статус автомобиля после попытки ремонта: {Car.Status}");
        }
    }
}

public class Program
{
    public static void Main()
    {
        Client[] clients = new Client[]
        {
            new Client("Анна"),

            new Client("Борис"),
            new Client("Виктория")
        };

        List<Mechanic> mechanics = new List<Mechanic>()
        {
            new Mechanic("Иван"),
            new Mechanic("Олег")
        };
        Car[] cars = new Car[]
                {
            new Car("VIN0001", "Toyota"),
            new Car("VIN0002", "Ford"),
            new Car("VIN0003", "Lada")
                };

        cars[2].SetStatus(CarStatus.CannotFix);

        Random rnd = new Random();

        foreach (var client in clients)
        {
            Car chosenCar = cars[rnd.Next(cars.Length)];
            Mechanic chosenMechanic = mechanics[rnd.Next(mechanics.Count)];

            try
            {
                WorkOrder order = client.CreateWorkOrder(chosenCar, chosenMechanic);
                order.Process();
            }
            catch (CannotRepairCarException ex)
            {
                Console.WriteLine($"[Ошибка] Клиент {client.Name}: {ex.Message}");
            }

        }
    }
}
