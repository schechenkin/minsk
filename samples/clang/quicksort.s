	.text
	.file	"quicksort.c"
	.globl	exch                            # -- Begin function exch
	.p2align	4, 0x90
	.type	exch,@function
exch:                                   # @exch
	.cfi_startproc
# %bb.0:                                # %entry
	pushq	%rbp
	.cfi_def_cfa_offset 16
	.cfi_offset %rbp, -16
	movq	%rsp, %rbp
	.cfi_def_cfa_register %rbp
	movq	%rdi, -8(%rbp)
	movq	%rsi, -16(%rbp)
	movq	%rdx, -24(%rbp)
	movq	-8(%rbp), %rax
	movq	-16(%rbp), %rcx
	movq	(%rax,%rcx,8), %rax
	movq	%rax, -32(%rbp)
	movq	-8(%rbp), %rax
	movq	-24(%rbp), %rcx
	movq	(%rax,%rcx,8), %rdx
	movq	-8(%rbp), %rax
	movq	-16(%rbp), %rcx
	movq	%rdx, (%rax,%rcx,8)
	movq	-32(%rbp), %rdx
	movq	-8(%rbp), %rax
	movq	-24(%rbp), %rcx
	movq	%rdx, (%rax,%rcx,8)
	popq	%rbp
	.cfi_def_cfa %rsp, 8
	retq
.Lfunc_end0:
	.size	exch, .Lfunc_end0-exch
	.cfi_endproc
                                        # -- End function
	.globl	partition                       # -- Begin function partition
	.p2align	4, 0x90
	.type	partition,@function
partition:                              # @partition
	.cfi_startproc
# %bb.0:                                # %entry
	pushq	%rbp
	.cfi_def_cfa_offset 16
	.cfi_offset %rbp, -16
	movq	%rsp, %rbp
	.cfi_def_cfa_register %rbp
	subq	$48, %rsp
	movq	%rdi, -8(%rbp)
	movq	%rsi, -16(%rbp)
	movq	%rdx, -24(%rbp)
	movq	-16(%rbp), %rax
	subq	$1, %rax
	movq	%rax, -32(%rbp)
	movq	-24(%rbp), %rax
	movq	%rax, -40(%rbp)
	movq	-8(%rbp), %rax
	movq	-24(%rbp), %rcx
	movq	(%rax,%rcx,8), %rax
	movq	%rax, -48(%rbp)
.LBB1_1:                                # %while.body
                                        # =>This Loop Header: Depth=1
                                        #     Child Loop BB1_2 Depth 2
                                        #     Child Loop BB1_5 Depth 2
	jmp	.LBB1_2
.LBB1_2:                                # %while.cond1
                                        #   Parent Loop BB1_1 Depth=1
                                        # =>  This Inner Loop Header: Depth=2
	movq	-8(%rbp), %rax
	movq	-32(%rbp), %rcx
	movq	%rcx, %rdx
	addq	$1, %rdx
	movq	%rdx, -32(%rbp)
	movq	8(%rax,%rcx,8), %rax
	cmpq	-48(%rbp), %rax
	jge	.LBB1_4
# %bb.3:                                # %while.body3
                                        #   in Loop: Header=BB1_2 Depth=2
	jmp	.LBB1_2
.LBB1_4:                                # %while.end
                                        #   in Loop: Header=BB1_1 Depth=1
	jmp	.LBB1_5
.LBB1_5:                                # %while.cond4
                                        #   Parent Loop BB1_1 Depth=1
                                        # =>  This Inner Loop Header: Depth=2
	movq	-48(%rbp), %rax
	movq	-8(%rbp), %rcx
	movq	-40(%rbp), %rdx
	movq	%rdx, %rsi
	addq	$-1, %rsi
	movq	%rsi, -40(%rbp)
	cmpq	-8(%rcx,%rdx,8), %rax
	jge	.LBB1_9
# %bb.6:                                # %while.body7
                                        #   in Loop: Header=BB1_5 Depth=2
	movq	-40(%rbp), %rax
	cmpq	-16(%rbp), %rax
	jne	.LBB1_8
# %bb.7:                                # %if.then
                                        #   in Loop: Header=BB1_1 Depth=1
	jmp	.LBB1_9
.LBB1_8:                                # %if.end
                                        #   in Loop: Header=BB1_5 Depth=2
	jmp	.LBB1_5
.LBB1_9:                                # %while.end9
                                        #   in Loop: Header=BB1_1 Depth=1
	movq	-32(%rbp), %rax
	cmpq	-40(%rbp), %rax
	jl	.LBB1_11
# %bb.10:                               # %if.then11
	jmp	.LBB1_12
.LBB1_11:                               # %if.end12
                                        #   in Loop: Header=BB1_1 Depth=1
	movq	-8(%rbp), %rdi
	movq	-32(%rbp), %rsi
	movq	-40(%rbp), %rdx
	callq	exch
	jmp	.LBB1_1
.LBB1_12:                               # %while.end13
	movq	-8(%rbp), %rdi
	movq	-32(%rbp), %rsi
	movq	-24(%rbp), %rdx
	callq	exch
	movq	-32(%rbp), %rax
	addq	$48, %rsp
	popq	%rbp
	.cfi_def_cfa %rsp, 8
	retq
.Lfunc_end1:
	.size	partition, .Lfunc_end1-partition
	.cfi_endproc
                                        # -- End function
	.globl	quicksort                       # -- Begin function quicksort
	.p2align	4, 0x90
	.type	quicksort,@function
quicksort:                              # @quicksort
	.cfi_startproc
# %bb.0:                                # %entry
	pushq	%rbp
	.cfi_def_cfa_offset 16
	.cfi_offset %rbp, -16
	movq	%rsp, %rbp
	.cfi_def_cfa_register %rbp
	subq	$32, %rsp
	movq	%rdi, -8(%rbp)
	movq	%rsi, -16(%rbp)
	movq	%rdx, -24(%rbp)
	movq	-24(%rbp), %rax
	cmpq	-16(%rbp), %rax
	jg	.LBB2_2
# %bb.1:                                # %if.then
	jmp	.LBB2_3
.LBB2_2:                                # %if.end
	movq	-8(%rbp), %rdi
	movq	-16(%rbp), %rsi
	movq	-24(%rbp), %rdx
	callq	partition
	movq	%rax, -32(%rbp)
	movq	-8(%rbp), %rdi
	movq	-16(%rbp), %rsi
	movq	-32(%rbp), %rdx
	subq	$1, %rdx
	callq	quicksort
	movq	-8(%rbp), %rdi
	movq	-32(%rbp), %rsi
	addq	$1, %rsi
	movq	-24(%rbp), %rdx
	callq	quicksort
.LBB2_3:                                # %return
	addq	$32, %rsp
	popq	%rbp
	.cfi_def_cfa %rsp, 8
	retq
.Lfunc_end2:
	.size	quicksort, .Lfunc_end2-quicksort
	.cfi_endproc
                                        # -- End function
	.globl	main                            # -- Begin function main
	.p2align	4, 0x90
	.type	main,@function
main:                                   # @main
	.cfi_startproc
# %bb.0:                                # %entry
	pushq	%rbp
	.cfi_def_cfa_offset 16
	.cfi_offset %rbp, -16
	movq	%rsp, %rbp
	.cfi_def_cfa_register %rbp
	subq	$32, %rsp
	movl	$0, -4(%rbp)
	movq	$10000000, -16(%rbp)            # imm = 0x989680
	movq	-16(%rbp), %rdi
	shlq	$3, %rdi
	callq	malloc@PLT
	movq	%rax, -24(%rbp)
	movq	-24(%rbp), %rdi
	movq	-16(%rbp), %rsi
	callq	fillArrayWithRandomNumbers@PLT
	movb	$0, %al
	callq	start_timer@PLT
	movq	-24(%rbp), %rdi
	movq	-16(%rbp), %rdx
	subq	$1, %rdx
	xorl	%eax, %eax
	movl	%eax, %esi
	callq	quicksort
	movb	$0, %al
	callq	stop_timer@PLT
	movl	$1, %eax
	addq	$32, %rsp
	popq	%rbp
	.cfi_def_cfa %rsp, 8
	retq
.Lfunc_end3:
	.size	main, .Lfunc_end3-main
	.cfi_endproc
                                        # -- End function
	.ident	"clang version 18.0.0git (https://github.com/llvm/llvm-project.git dc974573a8a2364f24ce69c75ad80ab30753fe9a)"
	.section	".note.GNU-stack","",@progbits
	.addrsig
	.addrsig_sym exch
	.addrsig_sym partition
	.addrsig_sym quicksort
	.addrsig_sym malloc
	.addrsig_sym fillArrayWithRandomNumbers
	.addrsig_sym start_timer
	.addrsig_sym stop_timer
