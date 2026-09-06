# Sensores ampliados (ES) — claves CUSTOM

Estas clases viven en `library/sensors/sensors_custom.py` y se usan en `theme.yaml`:

```yaml
STATS:
  CUSTOM:
    INTERVAL: 5
    CpuModel:
      TEXT: { SHOW: True, X: 16, Y: 72, ... }
```

| Clase | Qué muestra | Windows (LHM) | Linux |
|-------|-------------|----------------|-------|
| `CpuModel` | Modelo CPU | LibreHardwareMonitor | `/proc/cpuinfo` |
| `GpuModel` | Modelo GPU | LHM / get_gpu_name | `lspci` |
| `RamInfo` | Info/tamaño RAM | LHM Memory + psutil | psutil / meminfo |
| `DiskModel` | Disco/NVMe | LHM Storage | `/sys/class/nvme/*/model` |
| `NvmeTemp` | Temp almacenamiento | LHM Storage temp | hwmon NVMe |
| `CpuFanRpm` | RPM ventilador CPU | LHM Fan/Control | sysfs fans (it87, etc.) |
| `GpuFanRpm` | RPM ventilador GPU | LHM GPU Fan | — |
| `PreciseTime` | `HH:MM:SS` | datetime | datetime |
| `PreciseDate` | `dd mes` ES | datetime | datetime |
| `HostName` | Hostname | platform | platform |

## Temps / fans del núcleo

- **Windows**: `HW_SENSORS: AUTO` (LibreHardwareMonitor). Ejecutar **como administrador** para temps CPU/GPU/NVMe reales.
- LHM ahora prioriza **Tctl / Package / Core Average** y fans con etiqueta CPU (no solo `#2`).
- **Linux**: `HW_SENSORS: PYTHON` + lm-sensors/sysfs; `sensors_python` prioriza etiquetas Tctl/Package.

Temas actualizados de ejemplo: **AdminES**, **HorizonES** (modelos + NVMe + fan + reloj con segundos).
