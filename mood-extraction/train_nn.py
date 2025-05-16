# # import os
# # import torch
# # import torch.nn as nn
# # import torch.optim as optim
# # import pandas as pd
# # import numpy as np
# # from sklearn.model_selection import train_test_split
# # from sklearn.preprocessing import StandardScaler
# # from sklearn.metrics import mean_squared_error, r2_score
# #
# # # ─── 1) LOAD ───────────────────────────────────────────────────────────────────
# # df = pd.read_csv("output/features_reduced.csv", dtype={"song_id": str})
# # print(f"Found {len(df)} rows")
# # feature_cols = [
# #     "mfcc_mean", "chroma_mean", "melspec_mean",
# #     "lpc_mean", "pitch_mean", "centroid_mean",
# #     "bandwidth_mean", "contrast_mean", "rolloff_mean"
# # ]
# # target_cols = ["valence_mean", "arousal_mean"]
# #
# # X = df[feature_cols].values.astype(np.float32)
# # y = df[target_cols].values.astype(np.float32)
# #
# # # ─── 2) SPLIT ──────────────────────────────────────────────────────────────────
# # X_train, X_test, y_train, y_test = train_test_split(
# #     X, y, test_size=0.2, random_state=42
# # )
# #
# # # ─── 3) SCALE ──────────────────────────────────────────────────────────────────
# # scaler = StandardScaler().fit(X_train)
# # X_train = scaler.transform(X_train)
# # X_test = scaler.transform(X_test)
# #
# # # ─── 4) DATALOADER ─────────────────────────────────────────────────────────────
# # train_ds = torch.utils.data.TensorDataset(
# #     torch.from_numpy(X_train), torch.from_numpy(y_train)
# # )
# # train_loader = torch.utils.data.DataLoader(
# #     train_ds, batch_size=32, shuffle=True
# # )
# #
# #
# # # ─── 5) MODEL ──────────────────────────────────────────────────────────────────
# # class MoodNet(nn.Module):
# #     def __init__(self):
# #         super().__init__()
# #         self.net = nn.Sequential(
# #             nn.Linear(9, 128),
# #             nn.ReLU(),
# #             nn.Dropout(0.2),
# #             nn.Linear(128, 64),
# #             nn.ReLU(),
# #             nn.Dropout(0.2),
# #             nn.Linear(64, 2)
# #         )
# #
# #     def forward(self, x):
# #         return self.net(x)
# #
# #
# # model = MoodNet()
# # optimizer = optim.Adam(model.parameters(), lr=1e-3)
# # criterion = nn.MSELoss()
# # scheduler = optim.lr_scheduler.ReduceLROnPlateau(
# #     optimizer, mode="min", factor=0.5, patience=10
# # )
# #
# # # ─── 6) TRAIN & EVAL ───────────────────────────────────────────────────────────
# # best_epoch = 0
# # best_mse = 0
# #
# # for ep in range(1, 201):
# #     # — TRAIN —
# #     model.train()
# #     running_loss = 0.0
# #     for xb, yb in train_loader:
# #         pred = model(xb)
# #         loss = criterion(pred, yb)
# #         optimizer.zero_grad()
# #         loss.backward()
# #         optimizer.step()
# #         running_loss += loss.item() * xb.size(0)
# #     train_loss = running_loss / len(train_ds)
# #     scheduler.step(train_loss)
# #
# #     # — EVAL on TEST —
# #     model.eval()
# #     with torch.no_grad():
# #         xt = torch.from_numpy(X_test)
# #         yt = torch.from_numpy(y_test)
# #         ypred = model(xt).numpy()
# #         mse = mean_squared_error(y_test, ypred)
# #         r2 = r2_score(y_test, ypred)
# #
# #     # — LOG —
# #     if ep % 10 == 0 or ep == 1:
# #         print(f"Epoch {ep:3d}/200  |  "
# #               f"Train Loss {train_loss:.4f}  |  "
# #               f"Test MSE {mse:.4f}  |  "
# #               f"Test R² {r2:.4f}")
# #
# #     # — CHECKPOINT BEST —
# #     if mse < best_mse or best_mse == 0:
# #         best_mse = mse
# #         best_epoch = ep
# #         torch.save(model.state_dict(), "best_model.pth")
# #
# # print(f"Best epoch: {best_epoch}  |  "
# #       f"Best MSE: {best_mse:.4f}")
# #
# # print("✅ Saved best_model.pth")
# #
# # # ─── 7) EXPORT TO ONNX ─────────────────────────────────────────────────────────
# # # Reload the best checkpoint
# # model.load_state_dict(torch.load("best_model.pth"))
# # dummy = torch.randn(1, 9, dtype=torch.float32)
# # torch.onnx.export(
# #     model, dummy,
# #     "mood_model_pytorch.onnx",
# #     input_names=["input"],
# #     output_names=["output"],
# #     dynamic_axes={"input": {0: "batch"}, "output": {0: "batch"}},
# #     opset_version=11
# # )
# # print("✅ mood_model_pytorch.onnx written")
# #
# # print(scaler.mean_)
# # print(scaler.scale_)
#
# import os
# import torch
# import torch.nn as nn
# import torch.optim as optim
# import pandas as pd
# import numpy as np
# from sklearn.model_selection import train_test_split
# from sklearn.preprocessing import StandardScaler
# from sklearn.metrics import mean_squared_error, r2_score
#
# # ─── 1) LOAD ───────────────────────────────────────────────────────────────────
# df = pd.read_csv("output/features_reduced2.csv", dtype={"song_id": str})
# print(f"Found {len(df)} rows")
#
# # only these 6 features:
# feature_cols = [
#     "mfcc_mean",
#     "lpc_mean",
#     "pitch_mean",
#     "centroid_mean",
#     "bandwidth_mean",
#     "rolloff_mean"
# ]
#
# target_cols = ["valence_mean", "arousal_mean"]
#
# X = df[feature_cols].values.astype(np.float32)   # shape (N,6)
# y = df[target_cols].values.astype(np.float32)    # shape (N,2)
#
# # ─── 2) SPLIT ──────────────────────────────────────────────────────────────────
# X_train, X_test, y_train, y_test = train_test_split(
#     X, y, test_size=0.2, random_state=42
# )
#
# # ─── 3) SCALE ──────────────────────────────────────────────────────────────────
# scaler = StandardScaler().fit(X_train)
# X_train = scaler.transform(X_train)
# X_test  = scaler.transform(X_test)
#
# # save these for Unity
# print("SCALER MEANS:", scaler.mean_)
# print("SCALER SCALES:", scaler.scale_)
#
# # ─── 4) DATALOADER ─────────────────────────────────────────────────────────────
# train_ds     = torch.utils.data.TensorDataset(
#     torch.from_numpy(X_train), torch.from_numpy(y_train)
# )
# train_loader = torch.utils.data.DataLoader(
#     train_ds, batch_size=32, shuffle=True
# )
#
# # ─── 5) MODEL ──────────────────────────────────────────────────────────────────
# class MoodNet(nn.Module):
#     def __init__(self):
#         super().__init__()
#         self.net = nn.Sequential(
#             nn.Linear(6, 128),    # input = 6 features
#             nn.ReLU(),
#             nn.Dropout(0.2),
#             nn.Linear(128, 64),
#             nn.ReLU(),
#             nn.Dropout(0.2),
#             nn.Linear(64, 2)      # output = [valence, arousal]
#         )
#     def forward(self, x):
#         return self.net(x)
#
# model     = MoodNet()
# optimizer = optim.Adam(model.parameters(), lr=1e-3)
# criterion = nn.MSELoss()
# scheduler = optim.lr_scheduler.ReduceLROnPlateau(
#     optimizer, mode="min", factor=0.5, patience=10
# )
#
# # ─── 6) TRAIN & EVAL ───────────────────────────────────────────────────────────
# best_mse   = np.inf
# best_epoch = 0
#
# for ep in range(1, 201):
#     # — TRAIN —
#     model.train()
#     running_loss = 0.0
#     for xb, yb in train_loader:
#         pred = model(xb)
#         loss = criterion(pred, yb)
#         optimizer.zero_grad()
#         loss.backward()
#         optimizer.step()
#         running_loss += loss.item() * xb.size(0)
#     train_loss = running_loss / len(train_ds)
#     scheduler.step(train_loss)
#
#     # — TEST —
#     model.eval()
#     with torch.no_grad():
#         xt    = torch.from_numpy(X_test)
#         yt    = torch.from_numpy(y_test)
#         yhat  = model(xt).numpy()
#         mse   = mean_squared_error(y_test, yhat)
#         r2    = r2_score(y_test, yhat)
#
#     if ep % 10 == 0 or ep == 1:
#         print(f"Epoch {ep:3d}/200  |  "
#               f"Train L {train_loss:.4f}  |  "
#               f"Test MSE {mse:.4f}  |  R² {r2:.4f}")
#
#     # — checkpoint —
#     if mse < best_mse:
#         best_mse   = mse
#         best_epoch = ep
#         torch.save(model.state_dict(), "best_model.pth")
#
# print(f"\n🏁 Done. Best epoch {best_epoch}  |  Best MSE {best_mse:.4f}")
#
# # ─── 7) EXPORT TO ONNX ─────────────────────────────────────────────────────────
# model.load_state_dict(torch.load("best_model.pth"))
# dummy = torch.randn(1, 6, dtype=torch.float32)
# torch.onnx.export(
#     model, dummy,
#     "mood_model_reduced.onnx",
#     input_names=["input"],
#     output_names=["output"],
#     dynamic_axes={"input": {0: "batch"}, "output": {0: "batch"}},
#     opset_version=11
# )
# print("✅ Exported mood_model_reduced.onnx")

import os
import torch
import torch.nn as nn
import torch.optim as optim
import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import mean_squared_error, r2_score

# ─── 1) LOAD ───────────────────────────────────────────────────────────────────
df = pd.read_csv("output/features_reduced2.csv", dtype={"song_id": str})
print(f"Found {len(df)} rows")

# now seven features instead of six
feature_cols = [
    "mfcc_mean",
    "lpc_mean",
    "pitch_mean",
    "centroid_mean",
    "bandwidth_mean",
    "contrast_mean",   # newly added
    "rolloff_mean"
]

target_cols = ["valence_mean", "arousal_mean"]

X = df[feature_cols].values.astype(np.float32)   # shape (N,7)
y = df[target_cols].values.astype(np.float32)    # shape (N,2)

# ─── 2) SPLIT ──────────────────────────────────────────────────────────────────
X_train, X_test, y_train, y_test = train_test_split(
    X, y, test_size=0.2, random_state=42
)


# ─── 3) SCALE ──────────────────────────────────────────────────────────────────
scaler = StandardScaler().fit(X_train)
X_train = scaler.transform(X_train)
X_test  = scaler.transform(X_test)


# ─── 4) DATALOADER ─────────────────────────────────────────────────────────────
train_ds     = torch.utils.data.TensorDataset(
    torch.from_numpy(X_train), torch.from_numpy(y_train)
)
train_loader = torch.utils.data.DataLoader(
    train_ds, batch_size=32, shuffle=True
)

# ─── 5) MODEL ──────────────────────────────────────────────────────────────────
class MoodNet(nn.Module):
    def __init__(self):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(7, 128),    # input = 7 features now
            nn.ReLU(),
            nn.Dropout(0.2),
            nn.Linear(128, 64),
            nn.ReLU(),
            nn.Dropout(0.2),
            nn.Linear(64, 2)      # output = [valence, arousal]
        )
    def forward(self, x):
        return self.net(x)

model     = MoodNet()
optimizer = optim.Adam(model.parameters(), lr=1e-3)
criterion = nn.MSELoss()
scheduler = optim.lr_scheduler.ReduceLROnPlateau(
    optimizer, mode="min", factor=0.5, patience=10
)

# ─── 6) TRAIN & EVAL ───────────────────────────────────────────────────────────
best_mse   = np.inf
best_epoch = 0

for ep in range(1, 201):
    # — TRAIN —
    model.train()
    running_loss = 0.0
    for xb, yb in train_loader:
        pred = model(xb)
        loss = criterion(pred, yb)
        optimizer.zero_grad()
        loss.backward()
        optimizer.step()
        running_loss += loss.item() * xb.size(0)
    train_loss = running_loss / len(train_ds)
    scheduler.step(train_loss)

    # — TEST —
    model.eval()
    with torch.no_grad():
        xt    = torch.from_numpy(X_test)
        yt    = torch.from_numpy(y_test)
        yhat  = model(xt).numpy()
        mse   = mean_squared_error(y_test, yhat)
        r2    = r2_score(y_test, yhat)

    if ep % 10 == 0 or ep == 1:
        print(f"Epoch {ep:3d}/200  |  "
              f"Train L {train_loss:.4f}  |  "
              f"Test MSE {mse:.4f}  |  R² {r2:.4f}")

    # — checkpoint —
    if mse < best_mse:
        best_mse   = mse
        best_epoch = ep
        torch.save(model.state_dict(), "best_model.pth")

print(f"\n🏁 Done. Best epoch {best_epoch}  |  Best MSE {best_mse:.4f}")

# ─── 7) EXPORT TO ONNX ─────────────────────────────────────────────────────────
model.load_state_dict(torch.load("best_model.pth"))
dummy = torch.randn(1, 7, dtype=torch.float32)   # match 7 inputs
torch.onnx.export(
    model, dummy,
    "mood_model_reduced.onnx",
    input_names=["input"],
    output_names=["output"],
    dynamic_axes={"input": {0: "batch"}, "output": {0: "batch"}},
    opset_version=11
)
print("✅ Exported mood_model_reduced.onnx")
