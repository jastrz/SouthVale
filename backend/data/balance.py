#!/usr/bin/env python3
"""Troop balance report: efficiency per cost, training time, upkeep. Prints a table."""
import csv
import os

COST_COLS = ("CostWood", "CostClay", "CostIron", "CostBrewery")
HEADER = ("Type", "Atk", "Def", "Carry", "Speed", "Cost", "Atk/Cost", "Def/Cost", "Carry/Cost", "Atk/min", "Atk/Upkeep", "Def/Upkeep")


def training_secs(t: str) -> float:
    value, unit = float(t[:-1]), t[-1]
    return value * {"s": 1, "m": 60, "h": 3600, "d": 86400}[unit]


def main() -> None:
    path = os.path.join(os.path.dirname(__file__), "troops.csv")
    with open(path, newline="") as f:
        rows = list(csv.DictReader(f))

    table = []
    for r in rows:
        cost = sum(float(r[c]) for c in COST_COLS)
        table.append((
            r["Type"], int(r["Attack"]), int(r["Defense"]), int(r["CarryCapacity"]),
            int(r["Speed"]), int(cost),
            f"{float(r['Attack']) / cost:.3f}", f"{float(r['Defense']) / cost:.3f}",
            f"{float(r['CarryCapacity']) / cost:.3f}", f"{float(r['Attack']) / training_secs(r['TrainingTime']) * 60:.3f}",
            f"{float(r['Attack']) / float(r['Upkeep']):.3f}" if float(r["Upkeep"]) > 0 else "-",
            f"{float(r['Defense']) / float(r['Upkeep']):.3f}" if float(r["Upkeep"]) > 0 else "-",
        ))
    table.sort(key=lambda row: float(row[6]), reverse=True)

    widths = [max(len(str(row[i])) for row in table + [HEADER]) for i in range(len(HEADER))]
    print("  ".join(h.ljust(w) for h, w in zip(HEADER, widths)))
    for row in table:
        print("  ".join(str(v).ljust(w) for v, w in zip(row, widths)))


if __name__ == "__main__":
    main()
