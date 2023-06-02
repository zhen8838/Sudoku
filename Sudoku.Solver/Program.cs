
using System.IO;
using Google.OrTools.Sat;
namespace Sudoku.Solver;

public sealed class Program
{
    public static Task Main(string[] argv)
    {
        var file = argv[0];
        var input = new int[9, 9];
        var lines = File.ReadAllLines(file);
        for (int i = 0; i < 9; i++)
        {
            var line = lines[i].Split(", ");
            for (int j = 0; j < 9; j++)
            {
                input[i, j] = int.Parse(line[j]);
            }
        }

        Slove(input);

        return Task.CompletedTask;
    }


    public static void Slove(int[,] input)
    {
        int number = 9;
        var vars = Enumerable.Range(0, number).Select(i => Enumerable.Range(0, number).Select(j => new BoolVar[number]).ToArray()).ToArray();
        var model = new CpModel();

        // 1. build the bool vars
        for (int i = 0; i < number; i++)
        {
            for (int j = 0; j < number; j++)
            {
                for (int n = 0; n < number; n++)
                {
                    // the var indicate place number n on board [i,j]
                    vars[i][j][n] = model.NewBoolVar($"{i}_{j}_{n}");
                }
            }
        }

        // 2. add the unit clause for already exits number
        for (int i = 0; i < number; i++)
        {
            for (int j = 0; j < number; j++)
            {
                if (input[i, j] != 0)
                {
                    model.AddBoolOr(new[] { vars[i][j][input[i, j] - 1] });
                }
            }
        }

        var one = model.NewConstant(1);
        // 4. for each grid have one number
        for (int i = 0; i < number; i++)
        {
            for (int j = 0; j < number; j++)
            {
                model.Add(one == LinearExpr.Sum(vars[i][j]));
            }
        }

        var addLineConstraint = (IEnumerable<(int i, int j)> cells, int n) =>
        {
            model.Add(one == LinearExpr.Sum(cells.Select(c => vars[c.i][c.j][n])));
        };

        // 5. add valid
        for (int n = 0; n < number; n++)
        {
            // each line can't have duplicated number
            for (int i = 0; i < number; i++)
            {
                addLineConstraint(Enumerable.Range(0, number).Select(j => (i, j)), n);
            }

            // each colum can't have duplicated number
            for (int j = 0; j < number; j++)
            {
                addLineConstraint(Enumerable.Range(0, number).Select(i => (i, j)), n);
            }
            // each 3 x 3 can't have duplicated number
            for (int io = 0; io < number; io += 3)
            {
                for (int jo = 0; jo < number; jo += 3)
                {
                    var cells = new List<(int, int)>();
                    for (int ii = 0; ii < 3; ii++)
                    {
                        for (int ji = 0; ji < 3; ji++)
                        {
                            cells.Add((io + ii, jo + ji));
                        }
                    }
                    addLineConstraint(cells, n);
                }
            }
        }

        System.Console.WriteLine(model.Validate());

        CpSolver solver = new CpSolver();
        var status = solver.Solve(model);
        if (status is not (CpSolverStatus.Unknown or CpSolverStatus.Infeasible or CpSolverStatus.ModelInvalid))
        {
            for (int i = 0; i < number; i++)
            {
                System.Console.Write("{ ");
                for (int j = 0; j < number; j++)
                {
                    for (int n = 0; n < number; n++)
                    {
                        if (solver.BooleanValue(vars[i][j][n]))
                        {
                            System.Console.Write(n + 1);
                        }
                    }
                    System.Console.Write(", ");
                }
                System.Console.WriteLine("},");
            }
        }
        else
        {
            System.Console.WriteLine($"solver : {status}");
        }
    }
}