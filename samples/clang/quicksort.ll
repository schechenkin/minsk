; ModuleID = 'quicksort.c'
source_filename = "quicksort.c"
target datalayout = "e-m:e-p270:32:32-p271:32:32-p272:64:64-i64:64-f80:128-n8:16:32:64-S128"
target triple = "x86_64-pc-linux-gnu"

@.str = private unnamed_addr constant [16 x i8] c"numbers count:\0A\00", align 1
@.str.1 = private unnamed_addr constant [4 x i8] c"%ld\00", align 1

; Function Attrs: noinline nounwind optnone uwtable
define dso_local i64 @partition(i64* noundef %0, i64 noundef %1, i64 noundef %2) #0 {
  %4 = alloca i64*, align 8
  %5 = alloca i64, align 8
  %6 = alloca i64, align 8
  %7 = alloca i64, align 8
  %8 = alloca i64, align 8
  %9 = alloca i64, align 8
  store i64* %0, i64** %4, align 8
  store i64 %1, i64* %5, align 8
  store i64 %2, i64* %6, align 8
  %10 = load i64, i64* %5, align 8
  %11 = sub nsw i64 %10, 1
  store i64 %11, i64* %7, align 8
  %12 = load i64, i64* %6, align 8
  store i64 %12, i64* %8, align 8
  %13 = load i64*, i64** %4, align 8
  %14 = load i64, i64* %6, align 8
  %15 = getelementptr inbounds i64, i64* %13, i64 %14
  %16 = load i64, i64* %15, align 8
  store i64 %16, i64* %9, align 8
  br label %17

17:                                               ; preds = %3, %47
  br label %18

18:                                               ; preds = %26, %17
  %19 = load i64*, i64** %4, align 8
  %20 = load i64, i64* %7, align 8
  %21 = add nsw i64 %20, 1
  store i64 %21, i64* %7, align 8
  %22 = getelementptr inbounds i64, i64* %19, i64 %21
  %23 = load i64, i64* %22, align 8
  %24 = load i64, i64* %9, align 8
  %25 = icmp slt i64 %23, %24
  br i1 %25, label %26, label %27

26:                                               ; preds = %18
  br label %18, !llvm.loop !6

27:                                               ; preds = %18
  br label %28

28:                                               ; preds = %41, %27
  %29 = load i64, i64* %9, align 8
  %30 = load i64*, i64** %4, align 8
  %31 = load i64, i64* %8, align 8
  %32 = add nsw i64 %31, -1
  store i64 %32, i64* %8, align 8
  %33 = getelementptr inbounds i64, i64* %30, i64 %32
  %34 = load i64, i64* %33, align 8
  %35 = icmp slt i64 %29, %34
  br i1 %35, label %36, label %42

36:                                               ; preds = %28
  %37 = load i64, i64* %8, align 8
  %38 = load i64, i64* %5, align 8
  %39 = icmp eq i64 %37, %38
  br i1 %39, label %40, label %41

40:                                               ; preds = %36
  br label %42

41:                                               ; preds = %36
  br label %28, !llvm.loop !8

42:                                               ; preds = %40, %28
  %43 = load i64, i64* %7, align 8
  %44 = load i64, i64* %8, align 8
  %45 = icmp sge i64 %43, %44
  br i1 %45, label %46, label %47

46:                                               ; preds = %42
  br label %51

47:                                               ; preds = %42
  %48 = load i64*, i64** %4, align 8
  %49 = load i64, i64* %7, align 8
  %50 = load i64, i64* %8, align 8
  call void @exch(i64* noundef %48, i64 noundef %49, i64 noundef %50)
  br label %17

51:                                               ; preds = %46
  %52 = load i64*, i64** %4, align 8
  %53 = load i64, i64* %7, align 8
  %54 = load i64, i64* %6, align 8
  call void @exch(i64* noundef %52, i64 noundef %53, i64 noundef %54)
  %55 = load i64, i64* %7, align 8
  ret i64 %55
}

declare void @exch(i64* noundef, i64 noundef, i64 noundef) #1

; Function Attrs: noinline nounwind optnone uwtable
define dso_local void @quicksort(i64* noundef %0, i64 noundef %1, i64 noundef %2) #0 {
  %4 = alloca i64*, align 8
  %5 = alloca i64, align 8
  %6 = alloca i64, align 8
  %7 = alloca i64, align 8
  store i64* %0, i64** %4, align 8
  store i64 %1, i64* %5, align 8
  store i64 %2, i64* %6, align 8
  %8 = load i64, i64* %6, align 8
  %9 = load i64, i64* %5, align 8
  %10 = icmp sle i64 %8, %9
  br i1 %10, label %11, label %12

11:                                               ; preds = %3
  br label %25

12:                                               ; preds = %3
  %13 = load i64*, i64** %4, align 8
  %14 = load i64, i64* %5, align 8
  %15 = load i64, i64* %6, align 8
  %16 = call i64 @partition(i64* noundef %13, i64 noundef %14, i64 noundef %15)
  store i64 %16, i64* %7, align 8
  %17 = load i64*, i64** %4, align 8
  %18 = load i64, i64* %5, align 8
  %19 = load i64, i64* %7, align 8
  %20 = sub nsw i64 %19, 1
  call void @quicksort(i64* noundef %17, i64 noundef %18, i64 noundef %20)
  %21 = load i64*, i64** %4, align 8
  %22 = load i64, i64* %7, align 8
  %23 = add nsw i64 %22, 1
  %24 = load i64, i64* %6, align 8
  call void @quicksort(i64* noundef %21, i64 noundef %23, i64 noundef %24)
  br label %25

25:                                               ; preds = %12, %11
  ret void
}

; Function Attrs: noinline nounwind optnone uwtable
define dso_local i32 @main() #0 {
  %1 = alloca i32, align 4
  %2 = alloca i64, align 8
  %3 = alloca i64*, align 8
  store i32 0, i32* %1, align 4
  store i64 100, i64* %2, align 8
  %4 = call i32 (i8*, ...) @printf(i8* noundef getelementptr inbounds ([16 x i8], [16 x i8]* @.str, i64 0, i64 0))
  %5 = call i32 (i8*, ...) @__isoc99_scanf(i8* noundef getelementptr inbounds ([4 x i8], [4 x i8]* @.str.1, i64 0, i64 0), i64* noundef %2)
  %6 = load i64, i64* %2, align 8
  %7 = mul i64 %6, 8
  %8 = call noalias i8* @malloc(i64 noundef %7) #3
  %9 = bitcast i8* %8 to i64*
  store i64* %9, i64** %3, align 8
  %10 = load i64*, i64** %3, align 8
  %11 = load i64, i64* %2, align 8
  call void @fillArrayWithRandomNumbers(i64* noundef %10, i64 noundef %11)
  %12 = call i32 (...) @start_timer()
  %13 = load i64*, i64** %3, align 8
  %14 = load i64, i64* %2, align 8
  %15 = sub nsw i64 %14, 1
  call void @quicksort(i64* noundef %13, i64 noundef 0, i64 noundef %15)
  %16 = call i32 (...) @stop_timer()
  %17 = load i64*, i64** %3, align 8
  %18 = load i64, i64* %2, align 8
  call void @printNumbers(i64* noundef %17, i64 noundef %18)
  ret i32 1
}

declare i32 @printf(i8* noundef, ...) #1

declare i32 @__isoc99_scanf(i8* noundef, ...) #1

; Function Attrs: nounwind
declare noalias i8* @malloc(i64 noundef) #2

declare void @fillArrayWithRandomNumbers(i64* noundef, i64 noundef) #1

declare i32 @start_timer(...) #1

declare i32 @stop_timer(...) #1

declare void @printNumbers(i64* noundef, i64 noundef) #1

attributes #0 = { noinline nounwind optnone uwtable "frame-pointer"="all" "min-legal-vector-width"="0" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }
attributes #1 = { "frame-pointer"="all" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }
attributes #2 = { nounwind "frame-pointer"="all" "no-trapping-math"="true" "stack-protector-buffer-size"="8" "target-cpu"="x86-64" "target-features"="+cx8,+fxsr,+mmx,+sse,+sse2,+x87" "tune-cpu"="generic" }
attributes #3 = { nounwind }

!llvm.module.flags = !{!0, !1, !2, !3, !4}
!llvm.ident = !{!5}

!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!2 = !{i32 7, !"PIE Level", i32 2}
!3 = !{i32 7, !"uwtable", i32 1}
!4 = !{i32 7, !"frame-pointer", i32 2}
!5 = !{!"Ubuntu clang version 14.0.0-1ubuntu1.1"}
!6 = distinct !{!6, !7}
!7 = !{!"llvm.loop.mustprogress"}
!8 = distinct !{!8, !7}
