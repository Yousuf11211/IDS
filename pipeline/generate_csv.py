"""Generate synthetic inputs in the real inbox; predictions always come from the UBJ models."""
import argparse
import csv
from datetime import datetime, timezone
from pathlib import Path
import random
import uuid

from ids_pipeline.config import Config, ROOT
from ids_pipeline.schema import FEATURES, validate


def synthetic_row(rng, index, timestamp):
    # Vary ordinary, bursty and sparse flows. These are inputs, not ground-truth labels.
    profile = index % 3
    duration = rng.uniform(0.001, 0.2) if profile == 1 else rng.uniform(0.5, 60)
    fwd = rng.randint(100, 3000) if profile == 1 else rng.randint(1, 80)
    bwd = rng.randint(0, 2) if profile == 2 else rng.randint(1, fwd)
    count = fwd + bwd
    fwd_size, bwd_size = rng.randint(0, 1400), rng.randint(0, 1400)
    fwd_bytes, bwd_bytes = fwd * fwd_size, bwd * bwd_size
    payload = fwd_bytes + bwd_bytes
    iat = duration / max(count - 1, 1)
    row = {name: 0 for name in FEATURES}
    row.update(timestamp=timestamp, src_ip=f"192.0.2.{1 + index % 254}",
               dst_port=rng.choice([21, 22, 53, 80, 443, 3389, 8080]), duration=duration,
               packets_count=count, fwd_packets_count=fwd, bwd_packets_count=bwd,
               total_payload_bytes=payload, fwd_total_payload_bytes=fwd_bytes,
               bwd_total_payload_bytes=bwd_bytes, total_header_bytes=count * 20,
               fwd_total_header_bytes=fwd * 20, bwd_total_header_bytes=bwd * 20,
               mean_header_bytes=20, fwd_mean_header_bytes=20, bwd_mean_header_bytes=20,
               bytes_rate=payload / duration, fwd_bytes_rate=fwd_bytes / duration,
               bwd_bytes_rate=bwd_bytes / duration, packets_rate=count / duration,
               fwd_packets_rate=fwd / duration, bwd_packets_rate=bwd / duration,
               down_up_rate=bwd / fwd, fwd_init_win_bytes=rng.choice([0, 64, 128, 255, 256, 1024, 8192, 64240, 65535]),
               bwd_init_win_bytes=rng.choice([0, 64, 128, 255, 256, 1024, 8192, 64240, 65535]),
               subflow_fwd_packets=fwd, subflow_bwd_packets=bwd,
               subflow_fwd_bytes=fwd_bytes, subflow_bwd_bytes=bwd_bytes,
               delta_start=rng.uniform(0, 10), label="SyntheticCSV")
    fwd_header, bwd_header = rng.choice([20, 24, 28, 32, 40, 60]), rng.choice([20, 24, 28, 32, 40, 60])
    row.update(fwd_mean_header_bytes=fwd_header, bwd_mean_header_bytes=bwd_header,
               fwd_total_header_bytes=fwd * fwd_header, bwd_total_header_bytes=bwd * bwd_header,
               total_header_bytes=fwd * fwd_header + bwd * bwd_header,
               mean_header_bytes=(fwd * fwd_header + bwd * bwd_header) / count,
               payload_bytes_skewness=rng.uniform(-1, 25))
    row.update(payload_bytes_mean=payload / count, payload_bytes_max=max(fwd_size, bwd_size),
               payload_bytes_std=abs(fwd_size - bwd_size) / 2,
               fwd_payload_bytes_max=fwd_size, bwd_payload_bytes_max=bwd_size,
               fwd_payload_bytes_std=fwd_size / 4, bwd_payload_bytes_std=bwd_size / 4)
    row['payload_bytes_variance'] = row['payload_bytes_std'] ** 2
    row['payload_bytes_cov'] = row['payload_bytes_std'] / max(row['payload_bytes_mean'], 1)
    for name in FEATURES:
        if 'iat' in name or 'packets_delta_time' in name:
            if 'cov' in name:
                row[name] = 0.5
            elif 'skewness' in name:
                row[name] = rng.uniform(-1, 1)
            elif 'variance' in name:
                row[name] = (iat / 2) ** 2
            elif name.endswith('total'):
                row[name] = duration
            else:
                factor = 2 if 'max' in name else 0.5 if 'min' in name or 'std' in name else 1
                row[name] = iat * factor
        elif 'delta_len' in name:
            row[name] = rng.uniform(0, 1) if 'cov' in name else rng.uniform(-20, 20) if 'skewness' in name else rng.uniform(0, 100)
    fwd_ack = max(0, fwd - 1) if profile == 0 else rng.randint(0, fwd)
    bwd_ack = max(0, bwd - 1)
    row.update(fwd_ack_flag_counts=fwd_ack, bwd_ack_flag_counts=bwd_ack,
               ack_flag_counts=fwd_ack + bwd_ack,
               ack_flag_percentage_in_total=100 * (fwd_ack + bwd_ack) / count,
               fwd_ack_flag_percentage_in_total=100 * fwd_ack / count,
               bwd_ack_flag_percentage_in_total=100 * bwd_ack / count,
               fwd_ack_flag_percentage_in_fwd_packets=100 * fwd_ack / fwd,
               fwd_syn_flag_counts=fwd if profile == 2 else 1,
               bwd_syn_flag_counts=min(bwd, 1),
               fwd_fin_flag_counts=int(profile == 0), bwd_fin_flag_counts=int(profile == 0 and bwd > 0),
               fwd_psh_flag_counts=min(fwd_ack, max(0, fwd // 2)),
               fwd_rst_flag_counts=int(profile == 2), bwd_rst_flag_counts=0)
    for flag in ('syn', 'fin', 'rst'):
        row[f'{flag}_flag_counts'] = row[f'fwd_{flag}_flag_counts'] + row[f'bwd_{flag}_flag_counts']
    row['syn_flag_percentage_in_total'] = 100 * row['syn_flag_counts'] / count
    row['psh_flag_counts'] = row['fwd_psh_flag_counts']
    return row


def generate(config, rows=100, seed=42):
    if rows < 1:
        raise ValueError("rows must be positive")
    incoming = config.root / config.data_dir / 'incoming'
    incoming.mkdir(parents=True, exist_ok=True)
    timestamp = datetime.now(timezone.utc).isoformat()
    name = f"synthetic-{datetime.now(timezone.utc):%Y%m%dT%H%M%S}-{uuid.uuid4().hex[:8]}.csv"
    target = incoming / name
    temporary = target.with_suffix('.tmp')
    rng = random.Random(seed)
    columns = ['timestamp', 'src_ip'] + sorted([*FEATURES, 'label'])
    try:
        with temporary.open('x', encoding='utf-8', newline='') as stream:
            writer = csv.DictWriter(stream, fieldnames=columns)
            writer.writeheader()
            for index in range(rows):
                row = synthetic_row(rng, index, timestamp)
                validate(row)
                writer.writerow(row)
        temporary.replace(target)
        target.with_suffix('.csv.ready').touch(exist_ok=False)
    finally:
        temporary.unlink(missing_ok=True)
    return target


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--config', type=Path, default=ROOT / 'config/pipeline.toml')
    parser.add_argument('--rows', type=int, default=100)
    parser.add_argument('--seed', type=int, default=42)
    args = parser.parse_args(argv)
    if args.rows < 1:
        parser.error('--rows must be positive')
    target = generate(Config.load(args.config), args.rows, args.seed)
    print(f'Created {target} ({args.rows} synthetic rows) and its .ready marker.')
    print('Run python -m ids_pipeline run --once, or leave the continuous worker running.')
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
